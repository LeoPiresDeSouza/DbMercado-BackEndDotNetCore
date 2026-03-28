using System.Text.Json;
using DbMercado.Domain.Shared.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DbMercado.Api.Middlewares;

/// <summary>
/// Middleware global responsável por capturar exceções não tratadas durante o pipeline HTTP
/// e convertê-las em respostas padronizadas no formato <see cref="ProblemDetails"/>.
///
/// O middleware também cria um escopo estruturado de logging contendo informações da requisição,
/// como identificador de rastreamento, usuário autenticado, rota acessada, método HTTP,
/// endereço IP e User-Agent. Essas informações são automaticamente incluídas nos logs
/// gerados durante o processamento da requisição.
///
/// Quando uma exceção ocorre, ela é interceptada e mapeada para um código de status HTTP
/// apropriado e uma resposta JSON compatível com o padrão RFC 7807 (Problem Details).
/// Além disso, sempre são incluídos na resposta:
/// - <c>errorCode</c>: código interno de erro utilizado pela aplicação
/// - <c>traceId</c>: identificador da requisição para rastreamento nos logs
///
/// Caso a exceção seja do tipo <see cref="BusinessException"/>, todos os metadados adicionados
/// via método <c>With()</c> são automaticamente copiados para a propriedade
/// <see cref="ProblemDetails.Extensions"/>, permitindo o retorno de informações estruturadas
/// adicionais para o cliente ou sistemas de observabilidade.
///
/// <para><b>Mapeamento de exceções:</b></para>
/// <list type="bullet">
/// <item>
/// <description>
/// <see cref="IdentityAccessException"/> → HTTP 500 (Internal Server Error)
/// Utilizada para indicar falhas ao acessar ou manipular o sistema de identidade
/// da aplicação (ex.: criação de usuário, associação de claims, alteração de senha,
/// falhas do provedor de identidade).
/// </description>
/// </item>
/// <item>
/// <description>
/// <see cref="BusinessException"/> → HTTP 422 (Unprocessable Entity)
/// Representa violações de regras de negócio da aplicação.
/// Exemplo: tentativa de realizar uma operação inválida segundo as regras do domínio.
/// </description>
/// </item>
/// <item>
/// <description>
/// <see cref="DuplicateEntityException"/> → HTTP 409 (Conflict)
/// Indica tentativa de criação ou atualização de uma entidade que viola uma
/// restrição de unicidade (ex.: usuário com e-mail já existente).
/// </description>
/// </item>
/// <item>
/// <description>
/// <see cref="DbUpdateConcurrencyException"/> → HTTP 409 (Conflict)
/// Conflito de concorrência otimista: o registro foi alterado por outro usuário ou processo antes desta gravação.
/// </description>
/// </item>
/// <item>
/// <description>
/// <see cref="EntityNotFoundException"/> → HTTP 404 (Not Found)
/// Utilizada quando uma entidade requisitada não existe ou não pode ser localizada.
/// </description>
/// </item>
/// <item>
/// <description>
/// <see cref="UnauthorizedAccessException"/> → HTTP 401 (Unauthorized)
/// Indica que o usuário não possui credenciais válidas ou não está autenticado
/// para executar a operação solicitada.
/// </description>
/// </item>
/// <item>
/// <description>
/// Qualquer outra exceção não tratada → HTTP 500 (Internal Server Error).
/// Representa falhas inesperadas da aplicação.
/// </description>
/// </item>
/// </list>
///
/// <para>
/// Este middleware deve ser registrado no início do pipeline da aplicação,
/// antes de componentes como autenticação e autorização, garantindo que qualquer
/// exceção gerada posteriormente seja corretamente interceptada e tratada.
/// </para>
/// </summary>

public sealed class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var user = context.User;
        var userId = user?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                     ?? user?.FindFirst("sub")?.Value;

        var scope = new Dictionary<string, object?>
        {
            ["TraceId"] = System.Diagnostics.Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier,
            ["UserId"] = userId,
            ["UserName"] = user?.Identity?.Name,
            ["Path"] = context.Request.Path.Value,
            ["Method"] = context.Request.Method,
            ["RouteVals"] = context.Request.RouteValues,
            ["Ip"] = context.Connection.RemoteIpAddress?.ToString(),
            ["UserAgent"] = context.Request.Headers.UserAgent.ToString()
        };

        using (_logger.BeginScope(scope))
        {
            try
            {
                await _next(context);
            }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                _logger.LogInformation("Requisição cancelada pelo cliente.");
                context.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
            }
            catch (Exception ex)
            {
                if (context.Response.HasStarted)
                {
                    _logger.LogError(ex, "Exceção após início da resposta.");
                    throw;
                }

                await HandleExceptionAsync(context, ex);
            }
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (status, title, type, errorCode, detail, isBusiness) = MapException(ex);

        if (isBusiness)
            _logger.LogWarning(ex, "Erro de negócio. Code={ErrorCode}", errorCode);
        else
            _logger.LogError(ex, "Erro inesperado. Code={ErrorCode}", errorCode);

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Type = type,
            Detail = detail,
            Instance = context.Request.Path
        };

        problem.Extensions["errorCode"] = errorCode;
        problem.Extensions["traceId"] = context.TraceIdentifier;

        // Enriquecendo business exceptions com metadata adicional, se disponível

        if (ex is BusinessException bex)
        {
            foreach (var item in bex.Metadata)
                problem.Extensions[item.Key] = item.Value;
        }

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }

    private static (int status, string title, string type, string errorCode, string detail, bool isBusiness) MapException(Exception ex)
    {
        return ex switch
        {
            IdentityAccessException iex => (
                StatusCodes.Status500InternalServerError,
                "Erro no serviço de identidade",
                "https://httpstatuses.com/500",
                iex.ErrorCode,
                iex.Message,
                false
            ),

            BusinessException bex when ex is not DuplicateEntityException => (
                StatusCodes.Status422UnprocessableEntity,
                "Violação de regra de negócio",
                "https://httpstatuses.com/422",
                bex.ErrorCode,
                bex.Message,
                true
            ),

            DuplicateEntityException dex => (
                StatusCodes.Status409Conflict,
                "Entidade duplicada",
                "https://httpstatuses.com/409",
                dex.ErrorCode,
                dex.Message,
                true
            ),

            DbUpdateConcurrencyException => (
                StatusCodes.Status409Conflict,
                "Conflito de concorrência",
                "https://httpstatuses.com/409",
                "CONCURRENCY_CONFLICT",
                "Este registro foi alterado por outro usuário ou processo. Recarregue os dados e tente novamente.",
                true
            ),

            EntityNotFoundException nf => (
                StatusCodes.Status404NotFound,
                "Recurso não encontrado",
                "https://httpstatuses.com/404",
                "NOT_FOUND",
                nf.Message,
                false
            ),

            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                "Não autorizado",
                "https://httpstatuses.com/401",
                "UNAUTHORIZED",
                "Você não tem permissão para executar esta operação.",
                false
            ),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Erro interno",
                "https://httpstatuses.com/500",
                "INTERNAL_SERVER_ERROR",
                "Ocorreu um erro inesperado. Tente novamente mais tarde.",
                false
            )
        };
    }
}

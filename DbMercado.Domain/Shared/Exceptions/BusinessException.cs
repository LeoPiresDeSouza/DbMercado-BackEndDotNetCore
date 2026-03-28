using static DbMercado.Domain.Shared.ApplicationSettings.Permissions;

namespace DbMercado.Domain.Shared.Exceptions;

/// <summary>
/// Representa uma violação de regra de negócio da aplicação.
/// </summary>
/// <remarks>
/// Utilizada quando uma operação é válida tecnicamente, mas não pode ser executada
/// devido a regras do domínio da aplicação.
///
/// Os metadados podem ser enriquecidos utilizando o método <c>With()</c>,
/// permitindo retornar informações adicionais no <see cref="ProblemDetails"/> da API.
/// </remarks>
/// <example>
/// Exemplo de uso em um serviço de domínio ou aplicação:
/// <code>
/// if (pedido.Status == PedidoStatus.Finalizado)
/// {
///     throw new BusinessException("ORDER_ALREADY_FINISHED", "O pedido já foi finalizado.")
///         .With("PedidoId", pedido.Id)
///         .With("StatusAtual", pedido.Status);
/// }
/// </code>
/// </example>

public class BusinessException : Exception
{
    public string ErrorCode { get; }
    private readonly Dictionary<string, object?> _metadata = new();
    public IReadOnlyDictionary<string, object?> Metadata => _metadata;

    public BusinessException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public BusinessException(string errorCode, string message, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }



    public BusinessException With(string key, object? value)
    {
        _metadata[key] = value;
        return this;
    }
}

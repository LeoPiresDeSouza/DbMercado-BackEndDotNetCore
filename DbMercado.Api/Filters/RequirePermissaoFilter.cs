using DbMercado.Application.Administracao.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DbMercado.Api.Filters;

/// <summary>
/// Exige JWT autenticado e claim <c>Permissao</c> correspondente à funcionalidade + nome da permissão no cadastro.
/// </summary>
public sealed class RequirePermissaoFilter : IAsyncAuthorizationFilter
{
    private readonly string _funcionalidadeNomeNormalizado;
    private readonly string _permissaoNome;
    private readonly IPermissaoUsuarioResolver _resolver;

    public RequirePermissaoFilter(
        string funcionalidadeNomeNormalizado,
        string permissaoNome,
        IPermissaoUsuarioResolver resolver)
    {
        _funcionalidadeNomeNormalizado = funcionalidadeNomeNormalizado;
        _permissaoNome = permissaoNome;
        _resolver = resolver;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var ok = await _resolver.UsuarioPossuiPermissaoAsync(
            context.HttpContext.User,
            _funcionalidadeNomeNormalizado,
            _permissaoNome,
            context.HttpContext.RequestAborted);

        if (!ok)
            context.Result = new ForbidResult();
    }
}

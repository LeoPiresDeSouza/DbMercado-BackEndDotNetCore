using DbMercado.Application.Administracao;
using DbMercado.Application.Administracao.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;

namespace DbMercado.Api.Authorization;

public sealed class ChatMultilingueAcessarHandler : AuthorizationHandler<ChatMultilingueAcessarRequirement>
{
    private readonly IPermissaoUsuarioResolver _resolver;

    public ChatMultilingueAcessarHandler(IPermissaoUsuarioResolver resolver)
    {
        _resolver = resolver;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ChatMultilingueAcessarRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
            return;

        var ct = ResolveCancellationToken(context);

        var ok = await _resolver.UsuarioPossuiPermissaoAsync(
            context.User,
            ChatMultilinguePermissoesCatalogo.FuncionalidadeNomeNormalizado,
            ChatMultilinguePermissoesCatalogo.Acessar,
            ct);

        if (ok)
            context.Succeed(requirement);
    }

    private static CancellationToken ResolveCancellationToken(AuthorizationHandlerContext context)
    {
        if (context.Resource is HttpContext http)
            return http.RequestAborted;

        if (context.Resource is HubInvocationContext hub)
            return hub.Context.ConnectionAborted;

        return default;
    }
}

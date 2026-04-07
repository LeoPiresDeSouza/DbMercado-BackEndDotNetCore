using Microsoft.AspNetCore.Authorization;

namespace DbMercado.Api.Authorization;

/// <summary>Exige permissão <c>acessar</c> na funcionalidade <c>chatmultilingue</c> (mesma regra dos controllers de chat).</summary>
public sealed class ChatMultilingueAcessarRequirement : IAuthorizationRequirement
{
    public const string PolicyName = "ChatMultilingueAcessar";
}

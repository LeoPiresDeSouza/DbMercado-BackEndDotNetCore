using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace DbMercado.Api.RealTime;

/// <summary>
/// Alinha <see cref="IUserIdProvider"/> ao critério de resolução de ID do usuário usado no hub (JWT: NameIdentifier / sub),
/// para que <c>Clients.User(id)</c> no worker de tradução encontre as conexões SignalR.
/// </summary>
public sealed class ChatUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        var user = connection.User;
        var id = user?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrWhiteSpace(id))
            return id;

        id = user?.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (!string.IsNullOrWhiteSpace(id))
            return id;

        return user?.FindFirstValue("sub");
    }
}

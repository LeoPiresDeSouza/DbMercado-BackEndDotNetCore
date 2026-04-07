namespace DbMercado.Api.Hubs;

/// <summary>
/// Rota do <see cref="ChatHub"/>. Deve ser a mesma usada em <c>JwtBearerEvents.OnMessageReceived</c>
/// (query <c>access_token</c> no upgrade WebSocket — Etapa 3 do módulo chat).
/// </summary>
public static class ChatHubRoute
{
    public const string Path = "/hubs/chat";
}

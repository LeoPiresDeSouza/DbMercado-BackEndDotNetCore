using DbMercado.Api.Hubs;
using DbMercado.Application.Chat.Dtos;
using DbMercado.Application.Chat.HubClients;
using DbMercado.Application.Chat.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace DbMercado.Api.RealTime;

public sealed class ChatConviteSignalRNotifier : IChatConviteRealtimeNotifier
{
    private readonly IHubContext<ChatHub, IChatHubClient> _hubContext;
    private readonly ILogger<ChatConviteSignalRNotifier> _logger;

    public ChatConviteSignalRNotifier(
        IHubContext<ChatHub, IChatHubClient> hubContext,
        ILogger<ChatConviteSignalRNotifier> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotificarUsuarioConvidadoAsync(
        string invitedUserId,
        InviteResponse convite,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(invitedUserId))
            return;

        try
        {
            await _hubContext.Clients.User(invitedUserId).UserInvited(convite);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Falha ao enviar evento SignalR UserInvited para o usuário {UserId}.",
                invitedUserId);
        }
    }
}

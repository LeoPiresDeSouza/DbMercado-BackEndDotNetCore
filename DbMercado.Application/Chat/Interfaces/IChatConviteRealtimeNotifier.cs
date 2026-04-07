using DbMercado.Application.Chat.Dtos;

namespace DbMercado.Application.Chat.Interfaces;

/// <summary>Notificações em tempo real relacionadas a convites (ex.: SignalR).</summary>
public interface IChatConviteRealtimeNotifier
{
    /// <summary>Envia <c>UserInvited</c> ao usuário convidado, se houver conexão ativa.</summary>
    Task NotificarUsuarioConvidadoAsync(
        string invitedUserId,
        InviteResponse convite,
        CancellationToken cancellationToken = default);
}

using DbMercado.Application.Chat.Dtos;

namespace DbMercado.Application.Chat.Interfaces;

public interface IChatConviteService
{
    /// <summary>Aceita convite pendente, insere <c>ChatMember</c> e retorna a sala para o usuário.</summary>
    Task<RoomResponse> AceitarAsync(
        string identityUserId,
        string usuarioAuditoria,
        Guid inviteId,
        CancellationToken cancellationToken = default);

    /// <summary>Recusa convite pendente.</summary>
    Task RecusarAsync(
        string identityUserId,
        string usuarioAuditoria,
        Guid inviteId,
        CancellationToken cancellationToken = default);

    /// <summary>Lista convites pendentes e válidos dirigidos ao usuário (REST + SignalR).</summary>
    Task<IReadOnlyList<InviteResponse>> ListarPendentesParaUsuarioAsync(
        string identityUserId,
        CancellationToken cancellationToken = default);
}

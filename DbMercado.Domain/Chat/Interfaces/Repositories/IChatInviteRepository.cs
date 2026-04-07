using DbMercado.Domain.Chat.Entities;

namespace DbMercado.Domain.Chat.Interfaces.Repositories;

public interface IChatInviteRepository
{
    Task<bool> ExisteConvitePendenteAsync(
        Guid roomId,
        string invitedUserId,
        CancellationToken cancellationToken = default);

    /// <summary>Convite com rastreamento EF para atualização de status (aceitar/recusar).</summary>
    Task<ChatInviteEntity?> ObterPorIdComTrackingAsync(Guid id, CancellationToken cancellationToken = default);

    Task AdicionarAsync(
        string usuarioAutenticado,
        ChatInviteEntity entity,
        CancellationToken cancellationToken = default);

    Task AtualizarAsync(
        string usuarioAutenticado,
        ChatInviteEntity entity,
        CancellationToken cancellationToken = default);

    /// <summary>Convites pendentes, não expirados, para o usuário convidado; apenas salas ativas.</summary>
    Task<IReadOnlyList<ChatInviteEntity>> ListarPendentesNaoExpiradosParaConvidadoAsync(
        string invitedUserId,
        CancellationToken cancellationToken = default);
}

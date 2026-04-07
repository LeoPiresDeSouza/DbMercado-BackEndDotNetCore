using DbMercado.Domain.Chat.Entities;

namespace DbMercado.Domain.Chat.Interfaces.Repositories;

public interface IChatMemberRepository
{
    Task<bool> ExisteMembroNaSalaAsync(
        Guid roomId,
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>Retorna o <c>LanguagePref</c> do membro ou <c>null</c> se não pertencer à sala.</summary>
    Task<string?> ObterLanguagePrefMembroAsync(
        Guid roomId,
        string userId,
        CancellationToken cancellationToken = default);

    Task AdicionarAsync(
        string usuarioAutenticado,
        ChatMemberEntity entity,
        CancellationToken cancellationToken = default);

    Task<ChatMemberEntity?> ObterPorSalaEUsuarioComTrackingAsync(
        Guid roomId,
        string userId,
        CancellationToken cancellationToken = default);

    Task AtualizarAsync(
        string usuarioAutenticado,
        ChatMemberEntity entity,
        CancellationToken cancellationToken = default);

    /// <summary>Membros da sala com <c>UserId</c> e <c>LanguagePref</c> (para tradução e notificações SignalR).</summary>
    Task<IReadOnlyList<(string UserId, string LanguagePref)>> ListarUsuarioEIdiomaPorSalaAsync(
        Guid roomId,
        CancellationToken cancellationToken = default);
}

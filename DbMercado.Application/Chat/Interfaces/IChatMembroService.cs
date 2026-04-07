using DbMercado.Application.Chat.Dtos;

namespace DbMercado.Application.Chat.Interfaces;

public interface IChatMembroService
{
    /// <summary>Garante sala existente, ativa e com o usuário como membro (ex.: eventos SignalR sem persistência).</summary>
    Task ValidarMembroSalaAtivaAsync(
        string identityUserId,
        Guid roomId,
        CancellationToken cancellationToken = default);

    /// <summary>Atualiza <c>LanguagePref</c> do usuário autenticado na sala indicada.</summary>
    Task AtualizarIdiomaPreferidoAsync(
        string identityUserId,
        string usuarioAuditoria,
        Guid roomId,
        LanguageUpdateRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Idiomas-alvo distintos para tradução (membros da sala cujo <c>LanguagePref</c> difere do idioma de origem).</summary>
    Task<IReadOnlyList<string>> ObterIdiomasAlvoTraducaoAsync(
        Guid roomId,
        string sourceLang,
        CancellationToken cancellationToken = default);
}

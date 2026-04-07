using DbMercado.Domain.Chat;
using DbMercado.Domain.Chat.Interfaces.UnitsOfWork;

namespace DbMercado.Application.Chat;

/// <summary>Idiomas distintos necessários para tradução (preferência dos membros, exceto o idioma de origem da mensagem).</summary>
public static class ChatTranslationTargets
{
    public static async Task<IReadOnlyList<string>> ResolverParaSalaAsync(
        IUwChat uwChat,
        Guid roomId,
        string sourceLang,
        CancellationToken cancellationToken = default)
    {
        if (!LanguageCode.TryNormalize(sourceLang, out var srcCanon))
            srcCanon = sourceLang.Trim();

        var membros = await uwChat.Membros.ListarUsuarioEIdiomaPorSalaAsync(roomId, cancellationToken);

        return membros
            .Select(m => LanguageCode.TryNormalize(m.LanguagePref, out var c) ? c : null)
            .Where(c => c is not null && !string.Equals(c, srcCanon, StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .Cast<string>()
            .ToList();
    }
}

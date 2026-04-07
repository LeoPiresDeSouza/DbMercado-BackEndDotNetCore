namespace DbMercado.Application.Chat.Interfaces;

/// <summary>
/// Cache de textos já traduzidos (ex.: SHA256(conteúdo + idioma alvo) → tradução).
/// Implementação inicial em memória; futura troca por Redis via DI.
/// </summary>
public interface ITranslationCache
{
    Task<string?> GetAsync(string textHash, string targetLang, CancellationToken cancellationToken = default);

    Task SetAsync(string textHash, string targetLang, string translation, TimeSpan ttl, CancellationToken cancellationToken = default);
}

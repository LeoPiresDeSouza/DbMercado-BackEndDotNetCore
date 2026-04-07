namespace DbMercado.Application.Chat.Interfaces;

/// <summary>Traduz texto via OpenRouter (DeepSeek), conforme pipeline do chat multilíngue.</summary>
public interface IOpenRouterTranslationService
{
    /// <summary>
    /// Envia o conteúdo ao modelo com o system prompt do módulo (idiomas interpolados).
    /// </summary>
    Task<string> TranslateAsync(string content, string sourceLang, string targetLang, CancellationToken cancellationToken = default);
}

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using DbMercado.Application.Chat.Interfaces;
using DbMercado.CrossCutting.Settings;
using DbMercado.Domain.Shared.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DbMercado.Infrastructure.Chat.Translation;

/// <summary>
/// Cliente OpenRouter para <c>POST /api/v1/chat/completions</c> (modelo deepseek/deepseek-chat).
/// System prompt alinhado a <c>CHAT_MODULE.md</c>.
/// </summary>
public sealed class OpenRouterTranslationService : IOpenRouterTranslationService
{
    public const string HttpClientName = "OpenRouter";

    private const string ModelId = "deepseek/deepseek-chat";
    private const double Temperature = 0.1;
    private const int MaxTokens = 1000;

    /// <summary>Texto do CHAT_MODULE.md com placeholders {SourceLang} e {TargetLang}.</summary>
    private const string SystemPromptTemplate =
        """
        Você é um tradutor especializado em logística B2B e Supply Chain.
        Traduza o texto de {SourceLang} para {TargetLang}.
        Mantenha termos técnicos como '3PL', 'SKU', 'Fulfillment', 'CTE',
        'Romaneio', 'NF-e', 'DANFE', 'Lead Time', 'Cross-docking' sem traduzir
        se o contexto assim indicar.
        Retorne APENAS o texto traduzido, sem explicações, sem aspas.
        """;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptions<OpenRouterOptions> _options;
    private readonly ILogger<OpenRouterTranslationService> _logger;

    public OpenRouterTranslationService(
        IHttpClientFactory httpClientFactory,
        IOptions<OpenRouterOptions> options,
        ILogger<OpenRouterTranslationService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<string> TranslateAsync(
        string content,
        string sourceLang,
        string targetLang,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceLang);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetLang);

        var apiKey = _options.Value.ApiKey;
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "OpenRouter:ApiKey não configurado. Use User Secrets, variável OpenRouter__ApiKey ou appsettings locais não versionados.");
        }

        var systemPrompt = SystemPromptTemplate
            .Replace("{SourceLang}", sourceLang, StringComparison.Ordinal)
            .Replace("{TargetLang}", targetLang, StringComparison.Ordinal);

        var requestBody = new OpenRouterChatCompletionRequest
        {
            Model = ModelId,
            Temperature = Temperature,
            MaxTokens = MaxTokens,
            Messages =
            [
                new OpenRouterChatMessageDto { Role = "system", Content = systemPrompt },
                new OpenRouterChatMessageDto { Role = "user", Content = content }
            ]
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey.Trim());
        httpRequest.Content = JsonContent.Create(requestBody, options: JsonOptions);

        var client = _httpClientFactory.CreateClient(HttpClientName);
        using var response = await client.SendAsync(httpRequest, cancellationToken);
        var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "OpenRouter: falha final após retentativas Polly (HTTP {StatusCode}). Corpo: {Body}",
                (int)response.StatusCode,
                responseText.Length > 500 ? responseText[..500] + "…" : responseText);
            throw new BusinessException(
                "OPENROUTER_HTTP",
                $"A API de tradução retornou erro HTTP {(int)response.StatusCode}.");
        }

        OpenRouterChatCompletionResponse? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize<OpenRouterChatCompletionResponse>(responseText, JsonOptions);
        }
        catch (JsonException ex)
        {
            throw new BusinessException("OPENROUTER_JSON", "Resposta da API de tradução não é JSON válido.", ex);
        }

        var translated = parsed?.Choices?.FirstOrDefault()?.Message?.Content;
        if (string.IsNullOrWhiteSpace(translated))
        {
            throw new BusinessException("OPENROUTER_VAZIO", "A API de tradução não retornou texto.");
        }

        return translated.Trim();
    }
}

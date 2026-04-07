namespace DbMercado.CrossCutting.Settings;

/// <summary>Configuração da API OpenRouter (chat completions).</summary>
public sealed class OpenRouterOptions
{
    public const string SectionName = "OpenRouter";

    /// <summary>Bearer token da OpenRouter. Nunca versionar valor real no repositório.</summary>
    public string ApiKey { get; set; } = string.Empty;
}

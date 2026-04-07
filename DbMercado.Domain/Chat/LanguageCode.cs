namespace DbMercado.Domain.Chat;

/// <summary>Códigos BCP-47 suportados pelo chat multilíngue.</summary>
public static class LanguageCode
{
    public const string PtBr = "pt-BR";
    public const string En = "en";
    public const string ZhCn = "zh-CN";

    public static readonly HashSet<string> All = new(StringComparer.OrdinalIgnoreCase)
    {
        PtBr,
        En,
        ZhCn
    };

    /// <summary>Retorna o código canônico (BCP-47) se <paramref name="input"/> for um dos idiomas suportados.</summary>
    public static bool TryNormalize(string? input, out string canonical)
    {
        canonical = string.Empty;
        if (string.IsNullOrWhiteSpace(input))
            return false;

        var trimmed = input.Trim();
        if (string.Equals(trimmed, PtBr, StringComparison.OrdinalIgnoreCase))
        {
            canonical = PtBr;
            return true;
        }

        if (string.Equals(trimmed, En, StringComparison.OrdinalIgnoreCase))
        {
            canonical = En;
            return true;
        }

        if (string.Equals(trimmed, ZhCn, StringComparison.OrdinalIgnoreCase))
        {
            canonical = ZhCn;
            return true;
        }

        return false;
    }
}

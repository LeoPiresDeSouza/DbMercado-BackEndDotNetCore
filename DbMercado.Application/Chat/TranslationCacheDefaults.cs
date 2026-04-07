namespace DbMercado.Application.Chat;

/// <summary>TTL padrão do cache de traduções (alinhado a <c>CHAT_MODULE.md</c>).</summary>
public static class TranslationCacheDefaults
{
    public static readonly TimeSpan EntryTtl = TimeSpan.FromHours(2);
}

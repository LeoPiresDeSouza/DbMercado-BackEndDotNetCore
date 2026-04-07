using DbMercado.Application.Chat.Interfaces;
using DbMercado.Domain.Chat;
using DbMercado.Infrastructure.Shared.Interfaces;
using DeepBlues.Infrastructure.Providers;

namespace DbMercado.Infrastructure.Chat.Caching;

/// <summary>
/// Delega ao <see cref="IApplicationCachingService{TranslationTextCacheScope}"/> (IMemoryCache do projeto)
/// com TTL absoluto configurável; escopo isolado de invalidação do UoW de mensagens.
/// </summary>
public sealed class InMemoryTranslationCache : ITranslationCache
{
    private readonly IApplicationCachingService<TranslationTextCacheScope> _appCache;

    public InMemoryTranslationCache(IApplicationCachingService<TranslationTextCacheScope> appCache)
    {
        _appCache = appCache;
    }

    public Task<string?> GetAsync(string textHash, string targetLang, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var key = BuildKey(textHash, targetLang);
        string? value = _appCache.Get<string>(CachePolicy.LongTerm, key);
        return Task.FromResult<string?>(value);
    }

    public Task SetAsync(string textHash, string targetLang, string translation, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var key = BuildKey(textHash, targetLang);
        _appCache.Set(
            CachePolicy.LongTerm,
            translation,
            key,
            useToken: true,
            absoluteExpiration: ttl,
            slidingExpiration: null);
        return Task.CompletedTask;
    }

    private static string BuildKey(string textHash, string targetLang)
        => $"{textHash}\u001f{targetLang}";
}

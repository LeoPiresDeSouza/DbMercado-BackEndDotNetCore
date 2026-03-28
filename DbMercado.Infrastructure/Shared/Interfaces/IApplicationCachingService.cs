using DeepBlues.Infrastructure.Providers;

namespace DbMercado.Infrastructure.Shared.Interfaces;

public interface IApplicationCachingService<TEntity>
{
    Task<T> GetOrCreateAsync<T>(CachePolicy policy, string key, Func<Task<T>> factory, bool useToken = true, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null);
    T Get<T>(CachePolicy policy, string key);
    void Set<T>(CachePolicy policy, T value, string key, bool useToken = true, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null);
    void Remove(CachePolicy policy, string key);
    void InvalidateEntity();

}

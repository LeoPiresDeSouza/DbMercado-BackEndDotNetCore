using DbMercado.Domain;
using DbMercado.Infrastructure.Shared.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System.Collections.Concurrent;

namespace DeepBlues.Infrastructure.Providers;

#pragma warning disable 

/// <summary>
/// Define políticas de cache baseadas no comportamento do dado.
/// </summary>
/// <remarks>
/// LongTerm: Dados com baixa volatilidade, onde a consistência não é crítica (ex: tabelas de referência).
/// MediumTerm: Dados com volatilidade moderada, onde a consistência é importante, mas
/// ShortTerm: Dados altamente voláteis, onde a consistência é crítica (ex: resultados de queries).
/// </remarks>
public enum CachePolicy
{
    LongTerm,
    MediumTerm,
    ShortTerm
}



/// <summary>
/// Define os tempos padrão de expiração para cada política de cache.
/// </summary>
public class CachePolicyOptions
{
    public TimeSpan LongTermAbsolute { get; set; } = TimeSpan.FromHours(12);
    public TimeSpan MediumTermAbsolute { get; set; } = TimeSpan.FromMinutes(20);
    public TimeSpan ShortTermAbsolute { get; set; } = TimeSpan.FromSeconds(10);

    public TimeSpan? LongTermSliding { get; set; } = TimeSpan.FromHours(6);
    public TimeSpan? MediumTermSliding { get; set; } = TimeSpan.FromMinutes(10);
    public TimeSpan? ShortTermSliding { get; set; } = TimeSpan.FromSeconds(5);
}




/// <summary>
/// Serviço responsável pelo gerenciamento de cache da aplicação,
/// utilizando <see cref="IMemoryCache"/> com suporte a:
/// - Políticas de cache
/// - TTL configurável
/// - Jitter
/// - Invalidação por token
/// - Proteção contra cache stampede
/// </summary>
/// <typeparam name="TEntity">Entidade associada ao escopo de cache.</typeparam>
public class ApplicationCachingService<TEntity> : IApplicationCachingService<TEntity> where TEntity : class
{
    #region Membros públicos

    private readonly IMemoryCache _cache;
    private readonly CachePolicyOptions _options;
    private readonly string _prefix;
    
    /// <summary>
    /// Estrutura de controle de concorrência por chave de cache.
    /// Utilizada para evitar cache stampede.
    /// </summary>
    /// 
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    #endregion Membros públicos




    #region Ctor

    /// <summary>
    /// Inicializa o serviço de cache.
    /// </summary>
    public ApplicationCachingService(IMemoryCache cache)
    {
        _cache = cache;
        _options = new CachePolicyOptions();
        _prefix = ApplicationSettings.Application.ApplicationPrefix;
    }

    #endregion Ctor



    #region Métodos públicos

    /// <summary>
    /// Obtém um valor do cache ou o cria de forma segura em caso de ausência.
    /// Implementa proteção contra cache stampede.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="policy"></param>
    /// <param name="key"></param>
    /// <param name="factory"></param>
    /// <param name="useToken"></param>
    /// <param name="absoluteExpiration"></param>
    /// <param name="slidingExpiration"></param>
    /// <returns></returns>
    public async Task<T> GetOrCreateAsync<T>(CachePolicy policy,
                                             string key,
                                             Func<Task<T>> factory,
                                             bool useToken = true,
                                             TimeSpan? absoluteExpiration = null,
                                             TimeSpan? slidingExpiration = null)
    {
        var cacheKey = BuildKey(policy, key);

        if (_cache.TryGetValue(cacheKey, out T? value))
            return value;

        var semaphore = _locks.GetOrAdd(cacheKey, _ => new SemaphoreSlim(1, 1));

        await semaphore.WaitAsync();

        try
        {
            if (_cache.TryGetValue(cacheKey, out value))
                return value;

            value = await factory();

            Set(policy, value, key, useToken, absoluteExpiration, slidingExpiration);

            return value;
        }
        finally
        {
            semaphore.Release();
        }
    }



    /// <summary>
    /// Recupera um valor do cache.
    /// </summary>
    public T Get<T>(CachePolicy policy, string key)
    {
        var cacheKey = BuildKey(policy, key);
        return _cache.TryGetValue(cacheKey, out T value) ? value : default;
    }



    /// <summary>
    /// Adiciona um valor ao cache com suporte a TTL, jitter e token de invalidação.
    /// </summary>
    public void Set<T>( CachePolicy policy,
                        T value,
                        string key,
                        bool useToken = true,
                        TimeSpan? absoluteExpiration = null,
                        TimeSpan? slidingExpiration = null)
    {
        var options = new MemoryCacheEntryOptions();

        var abs = absoluteExpiration ?? GetAbsolute(policy);
        var slide = slidingExpiration ?? GetSliding(policy);

        abs = ApplyJitter(abs);

        options.SetAbsoluteExpiration(abs);

        if (slide.HasValue)
            options.SetSlidingExpiration(slide.Value);

        if (useToken)
        {
            var token = ApplicationCachingCancellationTokensFactory
                .Get(typeof(TEntity).FullName);

            options.AddExpirationToken(new CancellationChangeToken(token.Token));
        }

        _cache.Set(BuildKey(policy, key), value, options);
    }



    /// <summary>
    /// Remove uma entrada específica do cache.
    /// </summary>
    public void Remove(CachePolicy policy, string key)
    {
        _cache.Remove(BuildKey(policy, key));
    }



    /// <summary>
    /// Invalida todas as entradas de cache associadas à entidade.
    /// </summary>
    public void InvalidateEntity()
    {
        ApplicationCachingCancellationTokensFactory
            .Reset(typeof(TEntity).FullName);
    }

    #endregion Métodos públicos




    #region Métodos privados

    /// <summary>
    /// Constrói a chave padrão do cache.
    /// </summary>
    private string BuildKey(CachePolicy policy, string key)
        => $"{_prefix}:{typeof(TEntity).FullName}:{policy}:{key}";

    private TimeSpan GetAbsolute(CachePolicy policy)
        => policy switch
        {
            CachePolicy.LongTerm => _options.LongTermAbsolute,
            CachePolicy.MediumTerm => _options.MediumTermAbsolute,
            _ => _options.ShortTermAbsolute
        };

    private TimeSpan? GetSliding(CachePolicy policy)
        => policy switch
        {
            CachePolicy.LongTerm => _options.LongTermSliding,
            CachePolicy.MediumTerm => _options.MediumTermSliding,
            _ => _options.ShortTermSliding
        };

    /// <summary>
    /// Aplica jitter ao TTL para evitar expiração simultânea.
    /// </summary>
    private TimeSpan ApplyJitter(TimeSpan baseTtl)
    {
        var jitterFactor = Random.Shared.NextDouble() * 0.1;
        return TimeSpan.FromMilliseconds(baseTtl.TotalMilliseconds * (1 + jitterFactor));
    }

    #endregion Métodos privados
}

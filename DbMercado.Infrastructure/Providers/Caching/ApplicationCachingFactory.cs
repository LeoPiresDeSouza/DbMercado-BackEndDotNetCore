using DbMercado.Infrastructure.Shared.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using static DbMercado.Domain.Shared.ApplicationSettings;

namespace DeepBlues.Infrastructure.Providers;

/// <summary>
/// Factory responsávelpor construir uma instância do serviço de cache da aplicação. 
/// Expõe o método genérico GetApplicationCaching<TEntity>() responsável por criar ApplicationCachingService e 
/// injeta as dependências necessárias para o serviço de cache, como IMemoryCache, ILoggerFactory e IParametroService,
/// além de criá-la com o Tipo recebido, de forma que cada instância de cache conheça expicitamente para qual entidade
/// irá armazenar as informações no cache.
/// A informação da entidade é utilizada para a criação das chaves de cache, de forma a se segregar na memória entradas
/// relativas à mesma entidade.
/// Isso torna mais intuitivo a exclusão de chaves específicas de uma determinada entidade, principalmente nas chamadas
/// oriundas das UnitsOfWork.
/// </summary>
public class ApplicationCachingFactory: IApplicationCachingFactory
{
    #region Membros privados

    private readonly IMemoryCache _cache;

    #endregion Membros privados




    #region Ctor

    public ApplicationCachingFactory(IMemoryCache cache)
    {
        _cache = cache;
    }


    #endregion Ctor




    #region Métodos públicos

    /// <summary>
    /// Retorna uma instância de IApplicationCachingService para a entidade especificada.
    /// </summary>
    /// <typeparam name="TEntity">Type parameter</typeparam>
    /// <returns>IApplicationCachingService<TEntity></returns>
    public IApplicationCachingService<TEntity> GetApplicationCaching<TEntity>() where TEntity : class
    {

        IApplicationCachingService<TEntity> appCahce = new ApplicationCachingService<TEntity>(_cache);
        return appCahce;
    }

    #endregion Métodos públicos
}

using DbMercado.Domain.Administracao.Entities;
using DbMercado.Infrastructure.Administracao.Repositories.Administracao;
using DbMercado.Infrastructure.Shared.Data;
using DbMercado.Infrastructure.Shared.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DbMercado.Infrastructure.Shared.Repositories;

#pragma warning disable

public class RepositoryFactory : IRepositoryFactory
{
    #region Membros privados

    private readonly IServiceProvider _serviceProvider;
    private readonly IApplicationCachingFactory _cacheFactory;

    #endregion Membros privados




    #region Ctor

    public RepositoryFactory(IServiceProvider serviceProvider,
                             IApplicationCachingFactory cacheFactory)
    {
        _serviceProvider = serviceProvider;
        _cacheFactory = cacheFactory;
    }

    #endregion Ctor




    #region Métodos públicos

    public TRepository Create<TRepository>(AppDbContext context) where TRepository : class
    {
        // Atalho explícito: refresh token não usa cache de aplicação (consistência de revogação).
        if (typeof(TRepository) == typeof(RefreshTokenRepository))
            return (TRepository)(object)new RefreshTokenRepository(context);

        // Descubro qual é a entidade do repositório (ex: ModuloEntity)
        // Isso assume que os repositórios herdam de BaseRepository<TEntity>

        var entityType = typeof(TRepository).BaseType?.GetGenericArguments().FirstOrDefault();

        if (entityType != null)
        {
            // GetMethod("GetApplicationCaching") na classe concreta costuma falhar para método genérico; usa a interface.
            var getCaching = typeof(IApplicationCachingFactory)
                .GetMethods()
                .Single(static m =>
                    m.Name == nameof(IApplicationCachingFactory.GetApplicationCaching) && m.IsGenericMethodDefinition);
            var cacheService = getCaching.MakeGenericMethod(entityType).Invoke(_cacheFactory, null)
                ?? throw new InvalidOperationException("GetApplicationCaching retornou null.");

            // Invoke devolve object; ActivatorUtilities nem sempre casa com ctor (AppDbContext, IApplicationCachingService<T>).
            var instance = Activator.CreateInstance(typeof(TRepository), context, cacheService);
            if (instance is null)
            {
                throw new InvalidOperationException($"Não foi possível criar {typeof(TRepository).Name}.");
            }

            return (TRepository)instance;
        }

        // Caso o repositório não tenha uma entidade genérica, tenta criar só com o contexto

        return ActivatorUtilities.CreateInstance<TRepository>(_serviceProvider, context);
    }



    public TRepository Create<TRepository>() where TRepository : class
    {
        // Descubro qual é a entidade do repositório (ex: ModuloEntity)
        // Isso assume que os repositórios herdam de BaseRepository<TEntity>

        var entityType = typeof(TRepository).BaseType?.GetGenericArguments().FirstOrDefault();

        if (entityType != null)
        {

            // Crio o repositório passando o Contexto e o Cache Service que acabamos de criar
            // O ActivatorUtilities vai casar o 'context' e o 'cacheService' com o construtor do seu repositório.
            // O ActivatorUtilities é uma classe utilitária do pacote Microsoft.Extensions.DependencyInjection.
            // Ele é, essencialmente, uma "ponte" entre o mundo da Injeção de Dependência (DI) e a instanciação manual de objetos.
            // Ele permite criar instâncias de objetos, resolvendo suas dependências a partir do container de DI, mesmo que você esteja criando o objeto manualmente.
            //      Ele olha para todos os construtores da classe T.
            //      Ele tenta "casar" os parâmetros do construtor com os objetos que você passou manualmente no array de parâmetros.
            //      Se sobrar algum parâmetro no construtor que você não passou manualmente, ele vai até o IServiceProvider e tenta encontrar aquele serviço lá.
            //      Se ele encontrar tudo o que precisa, ele invoca o construtor e devolve o objeto pronto.

            return ActivatorUtilities.CreateInstance<TRepository>(_serviceProvider);
        }

        // Caso o repositório não tenha uma entidade genérica, tenta criar só com o contexto

        return ActivatorUtilities.CreateInstance<TRepository>(_serviceProvider);
    }

    #endregion Métodos públicos
}

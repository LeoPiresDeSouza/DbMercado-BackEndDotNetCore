namespace DbMercado.Infrastructure.Shared.Interfaces;


public interface IApplicationCachingFactory
{
    IApplicationCachingService<TEntity> GetApplicationCaching<TEntity>() where TEntity : class;
}

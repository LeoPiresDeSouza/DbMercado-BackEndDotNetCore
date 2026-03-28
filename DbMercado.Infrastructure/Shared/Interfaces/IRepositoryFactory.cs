using DbMercado.Infrastructure.Shared.Data;

namespace DbMercado.Infrastructure.Shared.Interfaces;

public interface IRepositoryFactory
{
    TRepository Create<TRepository>(AppDbContext context) where TRepository : class;
    TRepository Create<TRepository>() where TRepository : class;
}

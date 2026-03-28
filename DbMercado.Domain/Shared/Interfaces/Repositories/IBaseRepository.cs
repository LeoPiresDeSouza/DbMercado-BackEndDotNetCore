using System.Linq.Expressions;

namespace DbMercado.Domain.Shared.Interfaces.Repositories;

public interface IBaseRepository<T>
{
    Task<bool> ExistsAsync(long id);
    Task<int> CountAsync();
    Task<int> CountAsync(Expression<Func<T, bool>> predicate);
    Task<List<T>> GetAllAsync();
    Task<T> GetByIdAsync(long id);
    Task<IEnumerable<T>> FindCollectionAsync(Expression<Func<T, bool>> predicate);
    Task<T> FindFirstAsync(Expression<Func<T, bool>> predicate);
    Task AddAsync(string authenticatedUserName, T entity);
    Task UpdateAsync(string authenticatedUserName, T entity);
    Task DeleteAsync(T entity);
    Task DetachLocalAsync<TDetach>(Func<TDetach, bool> predicate) where TDetach : class;
}

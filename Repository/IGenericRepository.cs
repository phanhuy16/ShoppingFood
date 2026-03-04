#nullable enable
using System.Linq.Expressions;

namespace ShoppingFood.Repository
{
  public interface IGenericRepository<T> where T : class
  {
    Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, string includeProperties = "");
    Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> filter, string includeProperties = "");
    Task AddAsync(T entity);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);
    void Update(T entity);
  }
}

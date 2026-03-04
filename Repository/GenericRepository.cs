using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ShoppingFood.Repository
{
  public class GenericRepository<T> : IGenericRepository<T> where T : class
  {
    private readonly DataContext _db;
    internal DbSet<T> dbSet;

    public GenericRepository(DataContext db)
    {
      _db = db;
      this.dbSet = _db.Set<T>();
    }

    public async Task AddAsync(T entity)
    {
      await dbSet.AddAsync(entity);
    }

    public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, string includeProperties = "")
    {
      IQueryable<T> query = dbSet;

      if (filter != null)
      {
        query = query.Where(filter);
      }

      foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
      {
        query = query.Include(includeProp);
      }

      if (orderBy != null)
      {
        return await orderBy(query).ToListAsync();
      }
      return await query.ToListAsync();
    }

    public async Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> filter, string includeProperties = "")
    {
      IQueryable<T> query = dbSet;

      query = query.Where(filter);

      foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
      {
        query = query.Include(includeProp);
      }

      return await query.FirstOrDefaultAsync();
    }

    public void Remove(T entity)
    {
      dbSet.Remove(entity);
    }

    public void RemoveRange(IEnumerable<T> entities)
    {
      dbSet.RemoveRange(entities);
    }

    public void Update(T entity)
    {
      dbSet.Update(entity);
    }
  }
}

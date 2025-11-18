using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using RideShareAPI.Data;
using RideShareAPI.Repository.IRepository;

namespace RideShareAPI.Repository;

public class Repository<T>:IRepository<T> where T:class
{
    private readonly ApplicationDbContext _db;
    internal DbSet<T> _dbSet;

    public Repository(ApplicationDbContext db)
    {
        _db = db;
        _dbSet = _db.Set<T>();
    }


    public async Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null,bool tracked=true,params Expression<Func<T, object>>[] includeProperties)
    {
        IQueryable<T> query = _dbSet;
        if(!tracked)
        {
            query = query.AsNoTracking();
        }
        if (filter != null)
        {
            query = query.Where(filter);
        }

        foreach (var includeProperty in includeProperties)
        {
            query = query.Include(includeProperty);
        }
        return await query.ToListAsync();
    }

    public async Task<T> GetAsync(Expression<Func<T, bool>> filter, bool tracked = true, params Expression<Func<T, object>>[] includeProperties)
    {
        IQueryable<T> query = _dbSet;
        if(!tracked)
        {
            query = query.AsNoTracking();
        }
        if (filter != null)
        {
            query = query.Where(filter);
        }

        foreach (var includeProperty in includeProperties)
        {
            query = query.Include(includeProperty);
        }
        return await query.FirstOrDefaultAsync();
    }

    public async Task CreateAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await SaveAsync();    }

    public async Task RemoveAsync(T entity)
    {
        _dbSet.Remove(entity);
        await SaveAsync(); 
    }

    public async Task<T> UpdateAsync(T entity)
    {
         _dbSet.Update(entity);
        await SaveAsync();
        return entity;
    }

    public async Task SaveAsync()
    {
        await _db.SaveChangesAsync();
    }
}
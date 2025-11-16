using System.Linq.Expressions;

namespace RideShareAPI.Repository.IRepository;

public interface IRepository<T> where T:class
{
    Task <List<T>> GetAllAsync(Expression<Func<T,bool>>? filter = null,bool tracked=true,params Expression<Func<T, object>>[] includeProperties);
    Task<T> GetAsync(Expression<Func<T, bool>> filter,bool tracked=true, params Expression<Func<T, object>>[] includeProperties);
    Task CreateAsync(T entity);
    Task RemoveAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task SaveAsync();
    
}
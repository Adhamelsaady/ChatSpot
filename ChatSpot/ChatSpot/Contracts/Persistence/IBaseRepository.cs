using ChatSpot.Models.SQL;

namespace ChatSpot.Contracts.Persistence;


public interface IBaseRepository <T> where T : class
{
    Task<T> GetByIdAsync(Guid id);
    Task<T> GetByIdAsync(string id);
    Task<IReadOnlyList<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task<int> SaveChangesAsync();
    Task AddRangeAsync(IEnumerable<T> entities);

}
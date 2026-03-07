using ChatSpot.Contracts.Persistence;
using ChatSpot.Models.SQL;
using Microsoft.EntityFrameworkCore;

namespace ChatSpot.Repositories;

public class BaseRepository<T> : IBaseRepository<T> where T : class
{
    private readonly ChatSpotDbContext _db;
    public BaseRepository(ChatSpotDbContext db)
    {
        _db = db;
    }
    public async Task<T> GetByIdAsync(Guid id)
    {
        return await _db.Set<T>().FindAsync(id);
    }

    public async Task<T> GetByIdAsync(string id)
    {
        return await _db.Set<T>().FindAsync(id);
    }

    public async Task<IReadOnlyList<T>> GetAllAsync()
    {
        return await _db.Set<T>().ToListAsync();
    }

    public async Task<T> AddAsync(T entity)
    {
        await _db.Set<T>().AddAsync(entity);
        await  SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(T entity)
    {
        _db.Entry(entity).State = EntityState.Modified;
        await SaveChangesAsync();
    }

    public async Task DeleteAsync(T entity)
    {
        _db.Set<T>().Remove(entity);
        await _db.SaveChangesAsync();
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _db.SaveChangesAsync();
    }

    public async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await _db.Set<T>().AddRangeAsync(entities);
    }
}
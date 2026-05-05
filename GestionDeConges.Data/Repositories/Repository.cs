using GestionDeConges.Core.Interfaces;
using GestionDeConges.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GestionDeConges.Data.Repositories;

// ── Repository générique ──────────────────────────────────────────────────────
public class Repository<T>(AppDbContext ctx) : IRepository<T> where T : class
{
    protected readonly AppDbContext _ctx = ctx;
    protected readonly DbSet<T>    _set = ctx.Set<T>();

    public virtual async Task<T?> GetByIdAsync(int id) => await _set.FindAsync(id);

    public virtual async Task<IEnumerable<T>> GetAllAsync() => await _set.ToListAsync();

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        => await _set.Where(predicate).ToListAsync();

    public virtual async Task<T> AddAsync(T entity)
    {
        await _set.AddAsync(entity);
        return entity;
    }

    public virtual Task UpdateAsync(T entity)
    {
        _set.Update(entity);
        return Task.CompletedTask;
    }

    public virtual Task DeleteAsync(T entity)
    {
        _set.Remove(entity);
        return Task.CompletedTask;
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        => predicate is null
            ? await _set.CountAsync()
            : await _set.CountAsync(predicate);
}
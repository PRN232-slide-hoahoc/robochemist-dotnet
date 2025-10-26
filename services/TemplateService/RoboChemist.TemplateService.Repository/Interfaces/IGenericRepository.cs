using System.Linq.Expressions;

namespace RoboChemist.TemplateService.Repository.Interfaces;

/// <summary>
/// Generic repository interface for common CRUD operations
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public interface IGenericRepository<T> where T : class
{
    /// <summary>
    /// Get all entities
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Get entities with filter, orderBy, and includes
    /// </summary>
    Task<IEnumerable<T>> GetAsync(
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        string includeProperties = "");

    /// <summary>
    /// Get entity by ID
    /// </summary>
    Task<T?> GetByIdAsync(object id);

    /// <summary>
    /// Get first or default entity with filter
    /// </summary>
    Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> filter);

    /// <summary>
    /// Add new entity
    /// </summary>
    Task AddAsync(T entity);

    /// <summary>
    /// Add range of entities
    /// </summary>
    Task AddRangeAsync(IEnumerable<T> entities);

    /// <summary>
    /// Update entity
    /// </summary>
    void Update(T entity);

    /// <summary>
    /// Update range of entities
    /// </summary>
    void UpdateRange(IEnumerable<T> entities);

    /// <summary>
    /// Delete entity
    /// </summary>
    void Delete(T entity);

    /// <summary>
    /// Delete entity by ID
    /// </summary>
    Task DeleteByIdAsync(object id);

    /// <summary>
    /// Delete range of entities
    /// </summary>
    void DeleteRange(IEnumerable<T> entities);

    /// <summary>
    /// Count entities
    /// </summary>
    Task<int> CountAsync(Expression<Func<T, bool>>? filter = null);

    /// <summary>
    /// Check if entity exists
    /// </summary>
    Task<bool> ExistsAsync(Expression<Func<T, bool>> filter);
}


namespace RoboChemist.Shared.Common.GenericRepositories
{
    /// <summary>
    /// Generic repository interface for basic CRUD operations
    /// Compatible with PostgreSQL using Entity Framework Core
    /// </summary>
    /// <typeparam name="T">Entity type that must be a class</typeparam>
    public interface IGenericRepository<T> where T : class
    {
        /// <summary>
        /// Retrieves all entities from the database
        /// </summary>
        /// <returns>List of all entities</returns>
        Task<List<T>> GetAllAsync();

        /// <summary>
        /// Retrieves an entity by its integer ID
        /// </summary>
        /// <param name="id">The integer ID of the entity</param>
        /// <returns>The entity if found, otherwise null</returns>
        Task<T?> GetByIdAsync(int id);

        /// <summary>
        /// Retrieves an entity by its byte ID
        /// </summary>
        /// <param name="id">The byte ID of the entity</param>
        /// <returns>The entity if found, otherwise null</returns>
        Task<T?> GetByIdAsync(byte id);

        /// <summary>
        /// Retrieves an entity by its GUID ID (PostgreSQL UUID type)
        /// </summary>
        /// <param name="id">The GUID ID of the entity</param>
        /// <returns>The entity if found, otherwise null</returns>
        Task<T?> GetByIdAsync(Guid id);

        /// <summary>
        /// Retrieves entities that match the specified predicate
        /// Note: Predicate is executed client-side after loading data
        /// </summary>
        /// <param name="predicate">Filter condition</param>
        /// <returns>List of entities matching the predicate</returns>
        Task<List<T>> GetAsync(Func<T, bool> predicate);

        /// <summary>
        /// Creates a new entity in the database
        /// PostgreSQL will auto-generate ID if using SERIAL or IDENTITY
        /// </summary>
        /// <param name="entity">The entity to create</param>
        /// <returns>Number of affected records (1 if successful, 0 if failed)</returns>
        Task<int> CreateAsync(T entity);

        /// <summary>
        /// Updates an existing entity in the database
        /// </summary>
        /// <param name="entity">The entity to update</param>
        /// <returns>Number of affected records</returns>
        Task<int> UpdateAsync(T entity);

        /// <summary>
        /// Removes an entity from the database
        /// PostgreSQL will check foreign key constraints
        /// </summary>
        /// <param name="entity">The entity to remove</param>
        /// <returns>True if deletion was successful, false otherwise</returns>
        Task<bool> RemoveAsync(T entity);
    }
}

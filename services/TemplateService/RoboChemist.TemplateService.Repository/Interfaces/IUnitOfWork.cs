namespace RoboChemist.TemplateService.Repository.Interfaces;

/// <summary>
/// Unit of Work pattern interface for managing transactions
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Template repository
    /// </summary>
    ITemplateRepository Templates { get; }

    /// <summary>
    /// Order repository
    /// </summary>
    IOrderRepository Orders { get; }

    /// <summary>
    /// OrderDetail repository
    /// </summary>
    IOrderDetailRepository OrderDetails { get; }

    /// <summary>
    /// UserTemplate repository
    /// </summary>
    IUserTemplateRepository UserTemplates { get; }

    /// <summary>
    /// Save all changes to database
    /// </summary>
    Task<int> SaveChangesAsync();

    /// <summary>
    /// Begin transaction
    /// </summary>
    Task BeginTransactionAsync();

    /// <summary>
    /// Commit transaction
    /// </summary>
    Task CommitTransactionAsync();

    /// <summary>
    /// Rollback transaction
    /// </summary>
    Task RollbackTransactionAsync();
}


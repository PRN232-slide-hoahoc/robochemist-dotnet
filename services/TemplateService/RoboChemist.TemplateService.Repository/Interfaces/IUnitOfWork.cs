namespace RoboChemist.TemplateService.Repository.Interfaces;

/// <summary>
/// Unit of Work pattern interface for grouping repositories
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
}


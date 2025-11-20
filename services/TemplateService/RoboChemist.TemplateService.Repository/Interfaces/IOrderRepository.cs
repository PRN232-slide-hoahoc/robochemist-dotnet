using RoboChemist.TemplateService.Model.Models;
using RoboChemist.Shared.Common.GenericRepositories;

namespace RoboChemist.TemplateService.Repository.Interfaces;

/// <summary>
/// Order repository interface
/// </summary>
public interface IOrderRepository : IGenericRepository<Order>
{
    /// <summary>
    /// Get orders by user ID with OrderDetails and Template navigation properties
    /// </summary>
    Task<IEnumerable<Order>> GetOrdersByUserIdAsync(Guid userId);

    /// <summary>
    /// Get paginated orders with OrderDetails and Template navigation properties
    /// </summary>
    Task<(IEnumerable<Order> Orders, int TotalCount)> GetPagedOrdersWithDetailsAsync(int pageNumber, int pageSize);
}


using RoboChemist.TemplateService.Model.Models;

namespace RoboChemist.TemplateService.Repository.Interfaces;

/// <summary>
/// Order repository interface
/// </summary>
public interface IOrderRepository : IGenericRepository<Order>
{
    /// <summary>
    /// Get orders by user ID
    /// </summary>
    Task<IEnumerable<Order>> GetOrdersByUserIdAsync(Guid userId);

    /// <summary>
    /// Get order by order number
    /// </summary>
    Task<Order?> GetOrderByOrderNumberAsync(string orderNumber);
}


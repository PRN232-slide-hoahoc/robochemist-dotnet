using RoboChemist.TemplateService.Model.Models;

namespace RoboChemist.TemplateService.Repository.Interfaces;

/// <summary>
/// OrderDetail repository interface
/// </summary>
public interface IOrderDetailRepository : IGenericRepository<OrderDetail>
{
    /// <summary>
    /// Get order details by order ID
    /// </summary>
    Task<IEnumerable<OrderDetail>> GetOrderDetailsByOrderIdAsync(Guid orderId);
}


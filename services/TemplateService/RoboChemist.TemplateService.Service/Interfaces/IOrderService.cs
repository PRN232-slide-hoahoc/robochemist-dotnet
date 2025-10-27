using RoboChemist.TemplateService.Model.DTOs;
using RoboChemist.TemplateService.Model.Models;

namespace RoboChemist.TemplateService.Service.Interfaces;

/// <summary>
/// Order service interface for business logic
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// Create a new order
    /// </summary>
    Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request);

    /// <summary>
    /// Get order by ID
    /// </summary>
    Task<OrderResponse?> GetOrderByIdAsync(Guid orderId);

    /// <summary>
    /// Get order by order number
    /// </summary>
    Task<OrderResponse?> GetOrderByOrderNumberAsync(string orderNumber);

    /// <summary>
    /// Get all orders for a user
    /// </summary>
    Task<IEnumerable<OrderSummaryResponse>> GetUserOrdersAsync(Guid userId);

    /// <summary>
    /// Get all orders (admin)
    /// </summary>
    Task<PagedResult<OrderSummaryResponse>> GetAllOrdersAsync(PaginationParams paginationParams);

    /// <summary>
    /// Update order status
    /// </summary>
    Task<OrderResponse> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusRequest request);

    /// <summary>
    /// Cancel an order
    /// </summary>
    Task<OrderResponse> CancelOrderAsync(Guid orderId);

    /// <summary>
    /// Get order statistics by user
    /// </summary>
    Task<OrderStatistics> GetOrderStatisticsByUserAsync(Guid userId);
}

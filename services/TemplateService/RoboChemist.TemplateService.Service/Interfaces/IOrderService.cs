using RoboChemist.TemplateService.Model.DTOs;
using RoboChemist.TemplateService.Model.Models;
using RoboChemist.Shared.DTOs.Common;

namespace RoboChemist.TemplateService.Service.Interfaces;

/// <summary>
/// Order service interface for business logic
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// Create a new order
    /// </summary>
    Task<ApiResponse<OrderResponse>> CreateOrderAsync(CreateOrderRequest request);

    /// <summary>
    /// Get order by ID
    /// </summary>
    Task<ApiResponse<OrderResponse>> GetOrderByIdAsync(Guid orderId);

    /// <summary>
    /// Get order by order number
    /// </summary>
    Task<ApiResponse<OrderResponse>> GetOrderByOrderNumberAsync(string orderNumber);

    /// <summary>
    /// Get all orders for a user
    /// </summary>
    Task<ApiResponse<IEnumerable<OrderSummaryResponse>>> GetUserOrdersAsync(Guid userId);

    /// <summary>
    /// Get all orders (admin)
    /// </summary>
    Task<ApiResponse<PagedResult<OrderSummaryResponse>>> GetAllOrdersAsync(PaginationParams paginationParams);

    /// <summary>
    /// Update order status
    /// </summary>
    Task<ApiResponse<OrderResponse>> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusRequest request);

    /// <summary>
    /// Cancel an order
    /// </summary>
    Task<ApiResponse<OrderResponse>> CancelOrderAsync(Guid orderId);

    /// <summary>
    /// Get order statistics by user
    /// </summary>
    Task<ApiResponse<OrderStatistics>> GetOrderStatisticsByUserAsync(Guid userId);
}

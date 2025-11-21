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
    /// Get order by ID
    /// </summary>
    Task<ApiResponse<OrderResponse>> GetOrderByIdAsync(Guid orderId);

    /// <summary>
    /// Get all orders for a user
    /// </summary>
    Task<ApiResponse<IEnumerable<OrderSummaryResponse>>> GetUserOrdersAsync(Guid userId);

    /// <summary>
    /// Get all orders (admin)
    /// </summary>
    Task<ApiResponse<PagedResult<OrderSummaryResponse>>> GetAllOrdersAsync(PaginationParams paginationParams);
}

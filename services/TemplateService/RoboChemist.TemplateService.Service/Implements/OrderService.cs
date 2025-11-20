using Microsoft.EntityFrameworkCore;
using RoboChemist.TemplateService.Model.DTOs;
using RoboChemist.TemplateService.Model.Models;
using RoboChemist.TemplateService.Repository.Interfaces;
using RoboChemist.TemplateService.Service.Interfaces;
using RoboChemist.Shared.Common.Constants;
using RoboChemist.Shared.DTOs.Common;

namespace RoboChemist.TemplateService.Service.Implements;

/// <summary>
/// Order service implementation
/// </summary>
public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;

    public OrderService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<OrderResponse>> GetOrderByIdAsync(Guid orderId)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
        if (order == null)
        {
            return ApiResponse<OrderResponse>.ErrorResult($"Order with ID {orderId} not found");
        }

        var orderResponse = await MapOrderToResponseAsync(orderId);
        return ApiResponse<OrderResponse>.SuccessResult(orderResponse);
    }

    public async Task<ApiResponse<IEnumerable<OrderSummaryResponse>>> GetUserOrdersAsync(Guid userId)
    {
        var orders = await _unitOfWork.Orders.GetOrdersByUserIdAsync(userId);
        
        var orderSummaries = orders.Select(o => new OrderSummaryResponse
        {
            OrderId = o.OrderId,
            OrderNumber = o.OrderNumber,
            TotalAmount = o.TotalAmount,
            Status = o.Status,
            ItemCount = o.OrderDetails.Count,
            TemplateName = o.OrderDetails.FirstOrDefault()?.Template?.TemplateName,
            CreatedAt = o.CreatedAt
        });

        return ApiResponse<IEnumerable<OrderSummaryResponse>>.SuccessResult(orderSummaries);
    }

    public async Task<ApiResponse<PagedResult<OrderSummaryResponse>>> GetAllOrdersAsync(PaginationParams paginationParams)
    {
        var (orders, totalCount) = await _unitOfWork.Orders.GetPagedOrdersWithDetailsAsync(
            paginationParams.PageNumber,
            paginationParams.PageSize);

        var items = orders.Select(o => new OrderSummaryResponse
        {
            OrderId = o.OrderId,
            OrderNumber = o.OrderNumber,
            TotalAmount = o.TotalAmount,
            Status = o.Status,
            ItemCount = o.OrderDetails.Count,
            TemplateName = o.OrderDetails.FirstOrDefault()?.Template?.TemplateName,
            CreatedAt = o.CreatedAt
        }).ToList();

        var pagedResult = new PagedResult<OrderSummaryResponse>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = paginationParams.PageNumber,
            PageSize = paginationParams.PageSize
        };

        return ApiResponse<PagedResult<OrderSummaryResponse>>.SuccessResult(pagedResult);
    }

    // Helper methods
    private async Task<OrderResponse> MapOrderToResponseAsync(Guid orderId)
    {
        var order = await _unitOfWork.Orders
            .GetAllAsync()
            .ContinueWith(t => t.Result.FirstOrDefault(o => o.OrderId == orderId));

        if (order == null)
        {
            throw new InvalidOperationException($"Order with ID {orderId} not found");
        }

        return MapOrderToResponse(order);
    }

    private OrderResponse MapOrderToResponse(Order order)
    {
        return new OrderResponse
        {
            OrderId = order.OrderId,
            UserId = order.UserId,
            OrderNumber = order.OrderNumber,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            PaymentTransactionId = order.PaymentTransactionId,
            PaymentDate = order.PaymentDate,
            Notes = order.Notes,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            OrderDetails = order.OrderDetails.Select(od => new OrderDetailResponse
            {
                OrderDetailId = od.OrderDetailId,
                OrderId = od.OrderId,
                TemplateId = od.TemplateId,
                TemplateName = od.Template?.TemplateName ?? "Unknown",
                Subtotal = od.Subtotal,
                CreatedAt = od.CreatedAt
            }).ToList()
        };
    }
}

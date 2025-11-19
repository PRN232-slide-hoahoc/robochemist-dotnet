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

    public async Task<ApiResponse<OrderResponse>> CreateOrderAsync(CreateOrderRequest request)
    {
        // Validate templates exist and are active
        var templateIds = request.Items.Select(i => i.TemplateId).Distinct().ToList();
        var templates = new List<Template>();

        foreach (var templateId in templateIds)
        {
            var template = await _unitOfWork.Templates.GetByIdAsync(templateId);
            if (template == null)
            {
                return ApiResponse<OrderResponse>.ErrorResult($"Template with ID {templateId} not found");
            }
            if (!template.IsActive)
            {
                return ApiResponse<OrderResponse>.ErrorResult($"Template '{template.TemplateName}' is not available for purchase");
            }
            templates.Add(template);
        }

        // Create order
        var order = new Order
        {
            OrderId = Guid.NewGuid(),
            UserId = request.UserId,
            OrderNumber = GenerateOrderNumber(),
            Status = RoboChemistConstants.ORDER_STATUS_PENDING,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Create order details and calculate total
        decimal totalAmount = 0;
        var orderDetails = new List<OrderDetail>();

        foreach (var item in request.Items)
        {
            var template = templates.First(t => t.TemplateId == item.TemplateId);
            var orderDetail = new OrderDetail
            {
                OrderDetailId = Guid.NewGuid(),
                OrderId = order.OrderId,
                TemplateId = item.TemplateId,
                Subtotal = template.Price,
                CreatedAt = DateTime.UtcNow
            };

            totalAmount += template.Price;
            orderDetails.Add(orderDetail);
        }

        order.TotalAmount = totalAmount;

        // Save to database
        await _unitOfWork.Orders.CreateAsync(order);
        
        foreach (var detail in orderDetails)
        {
            await _unitOfWork.OrderDetails.CreateAsync(detail);
        }

        // Return response
        var orderResponse = await MapOrderToResponseAsync(order.OrderId);
        return ApiResponse<OrderResponse>.SuccessResult(orderResponse, "Order created successfully");
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

    public async Task<ApiResponse<OrderResponse>> GetOrderByOrderNumberAsync(string orderNumber)
    {
        var order = await _unitOfWork.Orders.GetOrderByOrderNumberAsync(orderNumber);
        if (order == null)
        {
            return ApiResponse<OrderResponse>.ErrorResult($"Order with number {orderNumber} not found");
        }

        var orderResponse = MapOrderToResponse(order);
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

    public async Task<ApiResponse<OrderResponse>> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusRequest request)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
        if (order == null)
        {
            return ApiResponse<OrderResponse>.ErrorResult($"Order with ID {orderId} not found");
        }

        // Validate status transition
        if (order.Status == RoboChemistConstants.ORDER_STATUS_CANCELLED)
        {
            return ApiResponse<OrderResponse>.ErrorResult("Cannot update status of a cancelled order");
        }

        if (order.Status == RoboChemistConstants.ORDER_STATUS_COMPLETED && request.Status != RoboChemistConstants.ORDER_STATUS_COMPLETED)
        {
            return ApiResponse<OrderResponse>.ErrorResult("Cannot change status of a completed order");
        }

        order.Status = request.Status;
        order.UpdatedAt = DateTime.UtcNow;

        if (request.Status == RoboChemistConstants.ORDER_STATUS_COMPLETED && !string.IsNullOrEmpty(request.PaymentTransactionId))
        {
            order.PaymentTransactionId = request.PaymentTransactionId;
            order.PaymentDate = DateTime.UtcNow;
        }

        await _unitOfWork.Orders.UpdateAsync(order);

        var orderResponse = await MapOrderToResponseAsync(orderId);
        return ApiResponse<OrderResponse>.SuccessResult(orderResponse, "Order status updated successfully");
    }

    public async Task<ApiResponse<OrderResponse>> CancelOrderAsync(Guid orderId)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
        if (order == null)
        {
            return ApiResponse<OrderResponse>.ErrorResult($"Order with ID {orderId} not found");
        }

        if (order.Status == RoboChemistConstants.ORDER_STATUS_COMPLETED)
        {
            return ApiResponse<OrderResponse>.ErrorResult("Cannot cancel a completed order");
        }

        if (order.Status == RoboChemistConstants.ORDER_STATUS_CANCELLED)
        {
            return ApiResponse<OrderResponse>.ErrorResult("Order is already cancelled");
        }

        order.Status = RoboChemistConstants.ORDER_STATUS_CANCELLED;
        order.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Orders.UpdateAsync(order);

        var orderResponse = await MapOrderToResponseAsync(orderId);
        return ApiResponse<OrderResponse>.SuccessResult(orderResponse, "Order cancelled successfully");
    }

    public async Task<ApiResponse<OrderStatistics>> GetOrderStatisticsByUserAsync(Guid userId)
    {
        var orders = await _unitOfWork.Orders.GetOrdersByUserIdAsync(userId);
        var orderList = orders.ToList();

        var statistics = new OrderStatistics
        {
            TotalOrders = orderList.Count,
            CompletedOrders = orderList.Count(o => o.Status == RoboChemistConstants.ORDER_STATUS_COMPLETED),
            PendingOrders = orderList.Count(o => o.Status == RoboChemistConstants.ORDER_STATUS_PENDING),
            CancelledOrders = orderList.Count(o => o.Status == RoboChemistConstants.ORDER_STATUS_CANCELLED),
            TotalSpent = orderList.Where(o => o.Status == RoboChemistConstants.ORDER_STATUS_COMPLETED).Sum(o => o.TotalAmount)
        };

        return ApiResponse<OrderStatistics>.SuccessResult(statistics);
    }

    // Helper methods
    private string GenerateOrderNumber()
    {
        // Format: ORD-YYYYMMDD-XXXXXX (random 6 digits)
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
        var randomPart = new Random().Next(100000, 999999);
        return $"ORD-{datePart}-{randomPart}";
    }

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
                Quantity = 1,
                UnitPrice = od.Subtotal,
                Subtotal = od.Subtotal
            }).ToList()
        };
    }
}

using Microsoft.EntityFrameworkCore;
using RoboChemist.TemplateService.Model.DTOs;
using RoboChemist.TemplateService.Model.Exceptions;
using RoboChemist.TemplateService.Model.Models;
using RoboChemist.TemplateService.Repository.Interfaces;
using RoboChemist.TemplateService.Service.Interfaces;
using RoboChemist.Shared.Common.Constants;

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

    public async Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request)
    {
        // Validate templates exist and are active
        var templateIds = request.Items.Select(i => i.TemplateId).Distinct().ToList();
        var templates = new List<Template>();

        foreach (var templateId in templateIds)
        {
            var template = await _unitOfWork.Templates.GetByIdAsync(templateId);
            if (template == null)
            {
                throw new NotFoundException($"Template with ID {templateId} not found");
            }
            if (!template.IsActive)
            {
                throw new BadRequestException($"Template '{template.TemplateName}' is not available for purchase");
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
        return await MapOrderToResponseAsync(order.OrderId);
    }

    public async Task<OrderResponse?> GetOrderByIdAsync(Guid orderId)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
        if (order == null)
        {
            return null;
        }

        return await MapOrderToResponseAsync(orderId);
    }

    public async Task<OrderResponse?> GetOrderByOrderNumberAsync(string orderNumber)
    {
        var order = await _unitOfWork.Orders.GetOrderByOrderNumberAsync(orderNumber);
        if (order == null)
        {
            return null;
        }

        return MapOrderToResponse(order);
    }

    public async Task<IEnumerable<OrderSummaryResponse>> GetUserOrdersAsync(Guid userId)
    {
        var orders = await _unitOfWork.Orders.GetOrdersByUserIdAsync(userId);
        
        return orders.Select(o => new OrderSummaryResponse
        {
            OrderId = o.OrderId,
            OrderNumber = o.OrderNumber,
            TotalAmount = o.TotalAmount,
            Status = o.Status,
            ItemCount = o.OrderDetails.Count,
            CreatedAt = o.CreatedAt
        });
    }

    public async Task<PagedResult<OrderSummaryResponse>> GetAllOrdersAsync(PaginationParams paginationParams)
    {
        var query = (await _unitOfWork.Orders.GetAllAsync())
            .OrderByDescending(o => o.CreatedAt);

        var totalCount = query.Count();
        var items = query
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .Select(o => new OrderSummaryResponse
            {
                OrderId = o.OrderId,
                OrderNumber = o.OrderNumber,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                ItemCount = o.OrderDetails.Count,
                CreatedAt = o.CreatedAt
            })
            .ToList();

        return new PagedResult<OrderSummaryResponse>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = paginationParams.PageNumber,
            PageSize = paginationParams.PageSize
        };
    }

    public async Task<OrderResponse> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusRequest request)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
        if (order == null)
        {
            throw new NotFoundException($"Order with ID {orderId} not found");
        }

        // Validate status transition
        if (order.Status == RoboChemistConstants.ORDER_STATUS_CANCELLED)
        {
            throw new BadRequestException("Cannot update status of a cancelled order");
        }

        if (order.Status == RoboChemistConstants.ORDER_STATUS_COMPLETED && request.Status != RoboChemistConstants.ORDER_STATUS_COMPLETED)
        {
            throw new BadRequestException("Cannot change status of a completed order");
        }

        order.Status = request.Status;
        order.UpdatedAt = DateTime.UtcNow;

        if (request.Status == RoboChemistConstants.ORDER_STATUS_COMPLETED && !string.IsNullOrEmpty(request.PaymentTransactionId))
        {
            order.PaymentTransactionId = request.PaymentTransactionId;
            order.PaymentDate = DateTime.UtcNow;
        }

        await _unitOfWork.Orders.UpdateAsync(order);

        return await MapOrderToResponseAsync(orderId);
    }

    public async Task<OrderResponse> CancelOrderAsync(Guid orderId)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
        if (order == null)
        {
            throw new NotFoundException($"Order with ID {orderId} not found");
        }

        if (order.Status == RoboChemistConstants.ORDER_STATUS_COMPLETED)
        {
            throw new BadRequestException("Cannot cancel a completed order");
        }

        if (order.Status == RoboChemistConstants.ORDER_STATUS_CANCELLED)
        {
            throw new BadRequestException("Order is already cancelled");
        }

        order.Status = RoboChemistConstants.ORDER_STATUS_CANCELLED;
        order.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Orders.UpdateAsync(order);

        return await MapOrderToResponseAsync(orderId);
    }

    public async Task<OrderStatistics> GetOrderStatisticsByUserAsync(Guid userId)
    {
        var orders = await _unitOfWork.Orders.GetOrdersByUserIdAsync(userId);
        var orderList = orders.ToList();

        return new OrderStatistics
        {
            TotalOrders = orderList.Count,
            CompletedOrders = orderList.Count(o => o.Status == RoboChemistConstants.ORDER_STATUS_COMPLETED),
            PendingOrders = orderList.Count(o => o.Status == RoboChemistConstants.ORDER_STATUS_PENDING),
            CancelledOrders = orderList.Count(o => o.Status == RoboChemistConstants.ORDER_STATUS_CANCELLED),
            TotalSpent = orderList.Where(o => o.Status == RoboChemistConstants.ORDER_STATUS_COMPLETED).Sum(o => o.TotalAmount)
        };
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
            throw new NotFoundException($"Order with ID {orderId} not found");
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
                TemplateId = od.TemplateId,
                TemplateName = od.Template?.TemplateName ?? "Unknown",
                Subtotal = od.Subtotal,
                CreatedAt = od.CreatedAt
            }).ToList()
        };
    }
}

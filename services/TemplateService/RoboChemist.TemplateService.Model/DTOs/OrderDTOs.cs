using System.ComponentModel.DataAnnotations;

namespace RoboChemist.TemplateService.Model.DTOs;

// Note: CreateOrderItemRequest and OrderDetailResponse are in OrderDetailDTOs.cs

#region Order Requests

/// <summary>
/// Request for creating a new order
/// </summary>
public class CreateOrderRequest
{
    [Required(ErrorMessage = "User ID is required")]
    public Guid UserId { get; set; }

    [Required(ErrorMessage = "At least one order item is required")]
    [MinLength(1, ErrorMessage = "Order must contain at least one item")]
    public List<CreateOrderItemRequest> Items { get; set; } = new();

    public string? Notes { get; set; }
}

/// <summary>
/// Request for updating order status
/// </summary>
public class UpdateOrderStatusRequest
{
    [Required(ErrorMessage = "Status is required")]
    [RegularExpression("^(pending|completed|cancelled)$", 
        ErrorMessage = "Status must be one of: pending, completed, cancelled")]
    public string Status { get; set; } = string.Empty;

    public string? PaymentTransactionId { get; set; }
}

#endregion

#region Order Responses

/// <summary>
/// Full order response with details
/// </summary>
public class OrderResponse
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? PaymentTransactionId { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<OrderDetailResponse> OrderDetails { get; set; } = new();
}

/// <summary>
/// Summary order response for lists
/// </summary>
public class OrderSummaryResponse
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public string? TemplateName { get; set; }
    public DateTime CreatedAt { get; set; }
}

#endregion


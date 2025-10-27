using System.ComponentModel.DataAnnotations;

namespace RoboChemist.TemplateService.Model.DTOs;

/// <summary>
/// DTO for creating a new order
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
/// DTO for order item in create request
/// </summary>
public class CreateOrderItemRequest
{
    [Required(ErrorMessage = "Template ID is required")]
    public Guid TemplateId { get; set; }
}

/// <summary>
/// DTO for order response
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
/// DTO for order detail response
/// </summary>
public class OrderDetailResponse
{
    public Guid OrderDetailId { get; set; }
    public Guid TemplateId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for updating order status
/// </summary>
public class UpdateOrderStatusRequest
{
    [Required(ErrorMessage = "Status is required")]
    [RegularExpression("^(pending|completed|cancelled)$", 
        ErrorMessage = "Status must be one of: pending, completed, cancelled")]
    public string Status { get; set; } = string.Empty;

    public string? PaymentTransactionId { get; set; }
}

/// <summary>
/// DTO for order summary (used in lists)
/// </summary>
public class OrderSummaryResponse
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

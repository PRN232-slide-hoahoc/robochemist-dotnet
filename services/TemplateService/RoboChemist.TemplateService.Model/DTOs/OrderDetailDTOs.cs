using System.ComponentModel.DataAnnotations;

namespace RoboChemist.TemplateService.Model.DTOs;

#region OrderDetail Requests

/// <summary>
/// Request for creating order item
/// </summary>
public class CreateOrderItemRequest
{
    [Required(ErrorMessage = "Template ID is required")]
    public Guid TemplateId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; } = 1;

    [Range(0, double.MaxValue, ErrorMessage = "Unit price must be non-negative")]
    public decimal UnitPrice { get; set; }
}

#endregion

#region OrderDetail Responses

/// <summary>
/// Order detail response
/// </summary>
public class OrderDetailResponse
{
    public Guid OrderDetailId { get; set; }
    public Guid OrderId { get; set; }
    public Guid TemplateId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
}

#endregion

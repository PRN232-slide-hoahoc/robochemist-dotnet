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
    public decimal Subtotal { get; set; }
    public DateTime CreatedAt { get; set; }
}

#endregion

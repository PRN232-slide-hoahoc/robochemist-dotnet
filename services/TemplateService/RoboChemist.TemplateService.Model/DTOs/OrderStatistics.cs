namespace RoboChemist.TemplateService.Model.DTOs;

/// <summary>
/// Order statistics DTO
/// </summary>
public class OrderStatistics
{
    public int TotalOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int PendingOrders { get; set; }
    public int CancelledOrders { get; set; }
    public decimal TotalSpent { get; set; }
}

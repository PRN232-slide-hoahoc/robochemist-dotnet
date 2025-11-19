namespace RoboChemist.TemplateService.Model.DTOs;

#region UserTemplate Requests

/// <summary>
/// Request to grant template access to user
/// </summary>
public class GrantTemplateAccessRequest
{
    /// <summary>
    /// Template ID to grant access
    /// </summary>
    public Guid TemplateId { get; set; }
}

/// <summary>
/// Request to purchase a template
/// </summary>
public class PurchaseTemplateRequest
{
    /// <summary>
    /// Template ID to purchase
    /// </summary>
    public Guid TemplateId { get; set; }
}

#endregion

#region UserTemplate Responses

/// <summary>
/// Response for user template - returns complete template information
/// </summary>
public class UserTemplateResponse
{
    public Guid TemplateId { get; set; }
    public string ObjectKey { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public int SlideCount { get; set; }
    public bool IsPremium { get; set; }
    public decimal Price { get; set; }
    public int DownloadCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
}

/// <summary>
/// Response for template purchase
/// </summary>
public class PurchaseTemplateResponse
{
    public Guid TransactionId { get; set; }
    public Guid TemplateId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal NewBalance { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime PurchasedAt { get; set; }
}

#endregion


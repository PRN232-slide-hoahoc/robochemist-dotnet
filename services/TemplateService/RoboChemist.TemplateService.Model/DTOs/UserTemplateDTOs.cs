namespace RoboChemist.TemplateService.Model.DTOs;

/// <summary>
/// Request to grant template access to user
/// </summary>
public class GrantTemplateAccessRequest
{
    /// <summary>
    /// Template ID to grant access
    /// </summary>
    public Guid TemplateId { get; set; }

    /// <summary>
    /// Access type: "free", "purchased", "subscription"
    /// </summary>
    public string AccessType { get; set; } = "free";

    /// <summary>
    /// Expiration date (optional, for subscriptions)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Usage limit (optional)
    /// </summary>
    public int? UsageLimit { get; set; }
}

/// <summary>
/// Response for user template operations
/// </summary>
public class UserTemplateResponse
{
    public Guid UserTemplateId { get; set; }
    public Guid UserId { get; set; }
    public Guid TemplateId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public string TemplateType { get; set; } = string.Empty;
    public string AccessType { get; set; } = string.Empty;
    public DateTime AcquiredAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int UsageCount { get; set; }
    public int? UsageLimit { get; set; }
    public bool IsActive { get; set; }
    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;
    public bool HasReachedLimit => UsageLimit.HasValue && UsageCount >= UsageLimit.Value;
}

/// <summary>
/// Request to increment usage count
/// </summary>
public class IncrementUsageRequest
{
    public Guid TemplateId { get; set; }
}

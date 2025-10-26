using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoboChemist.TemplateService.Model.Models;

[Table("user_templates")]
public class UserTemplate
{
    [Key]
    [Column("user_template_id")]
    public Guid UserTemplateId { get; set; }

    [Column("user_id")]
    [Required]
    public Guid UserId { get; set; }

    [Column("template_id")]
    [Required]
    public Guid TemplateId { get; set; }

    [Column("access_type")]
    [Required]
    [MaxLength(50)]
    public string AccessType { get; set; } = "free"; // purchased, free, subscription

    [Column("acquired_at")]
    public DateTime AcquiredAt { get; set; } = DateTime.UtcNow;

    [Column("expires_at")]
    public DateTime? ExpiresAt { get; set; }

    [Column("usage_count")]
    public int UsageCount { get; set; } = 0;

    [Column("usage_limit")]
    public int? UsageLimit { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    // Navigation property
    [ForeignKey("TemplateId")]
    public virtual Template Template { get; set; } = null!;
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoboChemist.TemplateService.Model.Models;

[Table("templates")]
public class Template
{
    [Key]
    [Column("template_id")]
    public Guid TemplateId { get; set; }

    [Column("object_key")]
    [Required]
    [MaxLength(500)]
    public string ObjectKey { get; set; } = string.Empty;
    
    [Column("template_name")]
    [Required]
    [MaxLength(255)]
    public string TemplateName { get; set; } = string.Empty;

    [Column("template_type")]
    [Required]
    [MaxLength(50)]
    public string TemplateType { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("thumbnail_url")]
    [MaxLength(500)]
    public string? ThumbnailUrl { get; set; }

    [Column("preview_url")]
    [MaxLength(500)]
    public string? PreviewUrl { get; set; }

    [Column("content_structure")]
    public string? ContentStructure { get; set; } // JSON stored as string

    [Column("slide_count")]
    public int SlideCount { get; set; }

    [Column("is_premium")]
    public bool IsPremium { get; set; }

    [Column("price", TypeName = "numeric(10,2)")]
    public decimal Price { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("download_count")]
    public int DownloadCount { get; set; } = 0;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Column("created_by")]
    public Guid? CreatedBy { get; set; }

    [Column("version")]
    public int Version { get; set; } = 1;

    // Navigation properties
    public virtual ICollection<UserTemplate> UserTemplates { get; set; } = new List<UserTemplate>();
    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}

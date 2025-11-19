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

    [Column("description")]
    public string? Description { get; set; }

    [Column("thumbnail_key")]
    [MaxLength(500)]
    public string? ThumbnailKey { get; set; }

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

    // Navigation properties
    public virtual ICollection<UserTemplate> UserTemplates { get; set; } = new List<UserTemplate>();
    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}

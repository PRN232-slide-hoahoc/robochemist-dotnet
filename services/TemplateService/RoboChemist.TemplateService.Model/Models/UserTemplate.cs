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

    // Navigation property
    [ForeignKey("TemplateId")]
    public virtual Template Template { get; set; } = null!;
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoboChemist.TemplateService.Model.Models;

[Table("order_details")]
public class OrderDetail
{
    [Key]
    [Column("order_detail_id")]
    public Guid OrderDetailId { get; set; }

    [Column("order_id")]
    [Required]
    public Guid OrderId { get; set; }

    [Column("template_id")]
    [Required]
    public Guid TemplateId { get; set; }

    [Column("subtotal", TypeName = "numeric(10,2)")]
    public decimal Subtotal { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("OrderId")]
    public virtual Order Order { get; set; } = null!;

    [ForeignKey("TemplateId")]
    public virtual Template Template { get; set; } = null!;
}

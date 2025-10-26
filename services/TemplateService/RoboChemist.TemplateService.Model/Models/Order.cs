using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoboChemist.TemplateService.Model.Models;

[Table("orders")]
public class Order
{
    [Key]
    [Column("order_id")]
    public Guid OrderId { get; set; }

    [Column("user_id")]
    [Required]
    public Guid UserId { get; set; }

    [Column("order_number")]
    [Required]
    [MaxLength(50)]
    public string OrderNumber { get; set; } = string.Empty;

    [Column("total_amount", TypeName = "numeric(10,2)")]
    public decimal TotalAmount { get; set; }

    [Column("status")]
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "pending"; // pending, completed, cancelled

    [Column("payment_transaction_id")]
    [MaxLength(100)]
    public string? PaymentTransactionId { get; set; }

    [Column("payment_date")]
    public DateTime? PaymentDate { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}

using Microsoft.EntityFrameworkCore;
using RoboChemist.TemplateService.Model.Models;

namespace RoboChemist.TemplateService.Model.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // DbSets for entities
    public virtual DbSet<Template> Templates { get; set; }
    public virtual DbSet<UserTemplate> UserTemplates { get; set; }
    public virtual DbSet<Order> Orders { get; set; }
    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure Template entity
        modelBuilder.Entity<Template>(entity =>
        {
            entity.HasKey(e => e.TemplateId);
            entity.ToTable("templates");
            entity.Property(e => e.TemplateId).HasColumnName("template_id");
            entity.HasIndex(e => e.TemplateName);
            entity.HasIndex(e => e.IsActive);
        });

        // Configure UserTemplate entity
        modelBuilder.Entity<UserTemplate>(entity =>
        {
            entity.HasKey(e => e.UserTemplateId);
            entity.ToTable("user_templates");
            entity.Property(e => e.UserTemplateId).HasColumnName("user_template_id");
            
            // Relationships
            entity.HasOne(e => e.Template)
                .WithMany(t => t.UserTemplates)
                .HasForeignKey(e => e.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.TemplateId);
        });

        // Configure Order entity
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId);
            entity.ToTable("orders");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            
            entity.HasIndex(e => e.OrderNumber)
                .IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Status);
        });

        // Configure OrderDetail entity
        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => e.OrderDetailId);
            entity.ToTable("order_details");
            entity.Property(e => e.OrderDetailId).HasColumnName("order_detail_id");
            
            // Relationships
            entity.HasOne(e => e.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Template)
                .WithMany(t => t.OrderDetails)
                .HasForeignKey(e => e.TemplateId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.TemplateId);
        });
        
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}


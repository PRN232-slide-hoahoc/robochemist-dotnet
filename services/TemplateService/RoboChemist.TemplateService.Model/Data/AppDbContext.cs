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

    // TODO: Add DbSet properties for your models here
    // Example: public virtual DbSet<Template> Templates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // TODO: Configure your entity models here

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}


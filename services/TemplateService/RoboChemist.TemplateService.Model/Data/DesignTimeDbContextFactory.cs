using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RoboChemist.TemplateService.Model.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // Connection string cho design-time operations
        // Load từ environment variable hoặc hardcode
        var connectionString = Environment.GetEnvironmentVariable("TEMPLATE_DB")
            ?? "Host=ep-snowy-cell-a19ensgj-pooler.ap-southeast-1.aws.neon.tech;Database=robochemist_templateservice;Username=neondb_owner;Password=npg_CeHyrLVF3pb6;Port=5432;SSL Mode=Require";

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(connectionString, b => 
            b.MigrationsAssembly("RoboChemist.TemplateService.Model"));

        return new AppDbContext(optionsBuilder.Options);
    }
}

using Microsoft.EntityFrameworkCore;
using RoboChemist.Shared.Common.Services.Implements;
using RoboChemist.Shared.Common.Services.Interfaces;
using RoboChemist.SlidesService.Model.Data;
using RoboChemist.SlidesService.Repository.Implements;
using RoboChemist.SlidesService.Repository.Interfaces;
using RoboChemist.SlidesService.Service.Implements;
using RoboChemist.SlidesService.Service.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Load .env file from solution root
var solutionRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".."));
var envPath = Path.Combine(solutionRoot, ".env");
Console.WriteLine($"[DEBUG] Looking for .env at: {envPath}");
Console.WriteLine($"[DEBUG] .env exists: {File.Exists(envPath)}");
DotNetEnv.Env.Load(envPath);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddHttpContextAccessor();

// Unit of Work and Repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Dependency Injection for Services
builder.Services.AddScoped <IGradeService, GradeService>();
builder.Services.AddScoped <ITopicService, TopicService>();
builder.Services.AddScoped <ISyllabusService, SyllabusService>();
builder.Services.AddScoped <ISlideService, SlideService>();

// Dependency Injection for common Services
builder.Services.AddScoped<ICommonUserService, CommonUserService>();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "RoboChemist Slides Service API",
        Version = "v1",
    });

    // Add JWT Authorization
    //options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    //{
    //    Name = "Authorization",
    //    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
    //    Scheme = "Bearer",
    //    BearerFormat = "JWT",
    //    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
    //    Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9\""
    //});

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });

    // Include XML comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(Environment.GetEnvironmentVariable("SLIDE_DB")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RoboChemist.ExamService.Model.Data;
using RoboChemist.ExamService.Repository.Implements;
using RoboChemist.ExamService.Repository.Interfaces;
using RoboChemist.ExamService.Service.HttpClients;
using RoboChemist.ExamService.Service.Implements;
using RoboChemist.ExamService.Service.Interfaces;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Load .env file from solution root
var solutionRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".."));
var envPath = Path.Combine(solutionRoot, ".env");
Console.WriteLine($"[DEBUG] Looking for .env at: {envPath}");
Console.WriteLine($"[DEBUG] .env exists: {File.Exists(envPath)}");
DotNetEnv.Env.Load(envPath);
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add HttpContextAccessor for token forwarding
builder.Services.AddHttpContextAccessor();

// JWT Authentication Configuration
var jwtKey = Environment.GetEnvironmentVariable("JWT_SECRET") ?? "your-secret-key-min-32-characters-long-for-security";
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "RoboChemist.AuthService";
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "RoboChemist.Client";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "RoboChemist Exam Service API", 
        Version = "v1",
        Description = "API for managing exam matrices, questions, and exam generation. " +
                      "This service handles question banks, exam templates, and automated exam generation.",
        Contact = new OpenApiContact
        {
            Name = "RoboChemist Team",
            Email = "support@robochemist.com"
        }
    });

    // Add JWT Bearer Authentication to Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Configure named HttpClient for each service (direct communication)
builder.Services.AddHttpClient("AuthService", client =>
{
    client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("AUTH_SERVICE_URL") ?? "https://localhost:7188");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient("SlidesService", client =>
{
    client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("SLIDES_SERVICE_URL") ?? "https://localhost:7205");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient("TemplateService", client =>
{
    client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("TEMPLATE_SERVICE_URL") ?? "https://localhost:7206");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient("WalletService", client =>
{
    client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("WALLET_SERVICE_URL") ?? "https://localhost:7100");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddScoped<IAuthServiceClient, AuthServiceClient>();
builder.Services.AddScoped<ISlidesServiceHttpClient, SlidesServiceHttpClient>();
builder.Services.AddScoped<ITemplateServiceClient, TemplateServiceClient>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(Environment.GetEnvironmentVariable("EXAM_DB")));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IMatrixService, MatrixService>();
builder.Services.AddScoped<IWordExportService, WordExportService>();
builder.Services.AddScoped<IExamService, ExamService>();
builder.Services.AddScoped<IWalletServiceHttpClient, WalletServiceHttpClient>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

Console.WriteLine($"[DEBUG] EXAM_DB: {Environment.GetEnvironmentVariable("EXAM_DB")}");

app.Run();

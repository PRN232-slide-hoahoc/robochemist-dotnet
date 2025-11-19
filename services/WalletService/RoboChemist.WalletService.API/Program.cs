using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RoboChemist.WalletService.API;
using RoboChemist.WalletService.Model.Data;
using RoboChemist.WalletService.Repository.Implements;
using RoboChemist.WalletService.Repository.Interfaces;
using RoboChemist.WalletService.Service.BackgroundServices;
using RoboChemist.WalletService.Service.HttpClients;
using RoboChemist.WalletService.Service.Implements;
using RoboChemist.WalletService.Service.Interfaces;
using System.Text;
using static RoboChemist.Shared.DTOs.WalletServiceDTOs.VNPayDTOs;

var builder = WebApplication.CreateBuilder(args);

// Load .env file from solution root
var solutionRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".."));
var envPath = Path.Combine(solutionRoot, ".env");
DotNetEnv.Env.Load(envPath);
builder.Configuration.AddEnvironmentVariables();

// HTTP Context Accessor
builder.Services.AddHttpContextAccessor();

// HTTP Client Factory
builder.Services.AddHttpClient("ApiGateway", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:ApiGateway:BaseUrl"] ?? "https://localhost:5001");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Database
builder.Services.AddDbContext<WalletDbContext>(options =>
    options.UseNpgsql(Environment.GetEnvironmentVariable("WALLET_DB")));

// Register DbContext as DbContext for repositories
builder.Services.AddScoped<DbContext>(provider => provider.GetRequiredService<WalletDbContext>());

// Unit of Work (repositories are created internally by UnitOfWork)
builder.Services.AddScoped<UnitOfWork>();
builder.Services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<UnitOfWork>());

// Dependency Injection for Services
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<IPaymentService,PaymentService>();
builder.Services.AddScoped<IAuthServiceClient,AuthServiceClient>();

// Background Services
builder.Services.AddHostedService<TransactionStatusUpdateService>();

var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET")
    ?? throw new Exception("JWT_SECRET not found in .env!");
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
    ?? throw new Exception("JWT_ISSUER not found in .env!");
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
    ?? throw new Exception("JWT_AUDIENCE not found in .env!");

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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "RoboChemist Wallet Service API",
        Version = "v1",
    });

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

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {your token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                        Array.Empty<string>()
                    }
                });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

//load variables VNPay save in .env
builder.Services.AddSingleton(new VNPayConfig
{
    TmnCode = Environment.GetEnvironmentVariable("VNP_TMN_CODE") ?? string.Empty,
    HashSecret = Environment.GetEnvironmentVariable("VNP_HASH_SECRET") ?? string.Empty,
    VnpayUrl = Environment.GetEnvironmentVariable("VNP_URL") ?? string.Empty,
    CallbackUrl = Environment.GetEnvironmentVariable("CALLBACK_URL") ?? string.Empty
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

using Microsoft.EntityFrameworkCore;
using RoboChemist.Shared.Common.Services.Implements;
using RoboChemist.Shared.Common.Services.Interfaces;
using RoboChemist.WalletService.Model.Data;
using RoboChemist.WalletService.Repository.Implements;
using RoboChemist.WalletService.Repository.Interfaces;
using RoboChemist.WalletService.Service.BackgroundServices;
using RoboChemist.WalletService.Service.Implements;
using RoboChemist.WalletService.Service.Interfaces;
using static RoboChemist.Shared.DTOs.WalletServiceDTOs.VNPayDTOs;

var builder = WebApplication.CreateBuilder(args);

// Load .env file from solution root
var solutionRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".."));
var envPath = Path.Combine(solutionRoot, ".env");
DotNetEnv.Env.Load(envPath);
builder.Configuration.AddEnvironmentVariables();

// Database
builder.Services.AddDbContext<WalletDbContext>(options =>
    options.UseNpgsql(Environment.GetEnvironmentVariable("WALLET_DB")));

// Register DbContext as DbContext for repositories
builder.Services.AddScoped<DbContext>(provider => provider.GetRequiredService<WalletDbContext>());

// Unit of Work (repositories are created internally by UnitOfWork)
builder.Services.AddScoped<UnitOfWork>();
builder.Services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<UnitOfWork>());

// Dependency Injection for Services
builder.Services.AddScoped<IWalletService,WalletService>();
builder.Services.AddScoped<IPaymentService,PaymentService>();

// Dependency Injection for common Services
builder.Services.AddScoped<ICommonUserService, CommonUserService>();

// Background Services
builder.Services.AddHostedService<TransactionStatusUpdateService>();

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

app.UseAuthorization();

app.MapControllers();

app.Run();

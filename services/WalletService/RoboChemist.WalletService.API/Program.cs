using Microsoft.EntityFrameworkCore;
using RoboChemist.WalletService.Model.Data;
using static RoboChemist.Shared.DTOs.WalletServiceDTOs.VNPayDTOs;

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
builder.Services.AddSwaggerGen();

//load variables VNPay save in .env
builder.Services.AddSingleton(new VNPayConfig
{
    TmnCode = Environment.GetEnvironmentVariable("VNP_TMN_CODE"),
    HashSecret = Environment.GetEnvironmentVariable("VNP_HASH_SECRET"),
    VnpayUrl = Environment.GetEnvironmentVariable("VNP_URL"),
    CallbackUrl = Environment.GetEnvironmentVariable("CALLBACK_URL")
});

// Database
builder.Services.AddDbContext<WalletDbContext>(options =>
    options.UseNpgsql(Environment.GetEnvironmentVariable("WALLET_DB")));

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

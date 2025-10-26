using System.Reflection;
using Amazon.Runtime;
using Amazon.S3;
using Microsoft.EntityFrameworkCore;
using RoboChemist.TemplateService.API.Filters;
using RoboChemist.TemplateService.API.Middleware;
using RoboChemist.TemplateService.Model.Data;
var builder = WebApplication.CreateBuilder(args);

var solutionRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".."));
var envPath = Path.Combine(solutionRoot, ".env");

Console.WriteLine($"Looking for .env at: {envPath}");
if (File.Exists(envPath))
{
    DotNetEnv.Env.Load(envPath);
    Console.WriteLine("Successfully loaded .env file");
}
else
{
    Console.WriteLine("WARNING: .env file not found!");
}
builder.Configuration.AddEnvironmentVariables();

// Configure CORS
var allowedOriginsString = builder.Configuration["ALLOWED_ORIGINS"];
var allowedOrigins = !string.IsNullOrEmpty(allowedOriginsString)
    ? allowedOriginsString.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    : new[] { "http://localhost:3000", "http://localhost:5173" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Add Controllers with Validation Filter
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidateModelStateFilter>();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Template Service API",
        Version = "v1",
        Description = "API quản lý templates - Upload, Download, và quản lý file PowerPoint"
    });

    // Enable XML comments for Swagger documentation
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    // Configure file upload support
    c.MapType<IFormFile>(() => new Microsoft.OpenApi.Models.OpenApiSchema
    {
        Type = "string",
        Format = "binary"
    });
});

var dbConnectionString = builder.Configuration["TEMPLATE_DB"];
if (string.IsNullOrEmpty(dbConnectionString))
{
    throw new InvalidOperationException("LỖI CẤU HÌNH: Không tìm thấy 'TEMPLATE_DB' trong tệp .env");
}
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(dbConnectionString,
        b => b.MigrationsAssembly("RoboChemist.TemplateService.Model")));

// Register Unit of Work and Repositories
builder.Services.AddScoped<RoboChemist.TemplateService.Repository.Interfaces.IUnitOfWork, RoboChemist.TemplateService.Repository.Implements.UnitOfWork>();
builder.Services.AddScoped<RoboChemist.TemplateService.Repository.Interfaces.ITemplateRepository, RoboChemist.TemplateService.Repository.Implements.TemplateRepository>();
builder.Services.AddScoped<RoboChemist.TemplateService.Repository.Interfaces.IOrderRepository, RoboChemist.TemplateService.Repository.Implements.OrderRepository>();
builder.Services.AddScoped<RoboChemist.TemplateService.Repository.Interfaces.IOrderDetailRepository, RoboChemist.TemplateService.Repository.Implements.OrderDetailRepository>();
builder.Services.AddScoped<RoboChemist.TemplateService.Repository.Interfaces.IUserTemplateRepository, RoboChemist.TemplateService.Repository.Implements.UserTemplateRepository>();

// Register Services
builder.Services.AddScoped<RoboChemist.TemplateService.Service.Interfaces.ITemplateService, RoboChemist.TemplateService.Service.Implements.TemplateService>();
builder.Services.AddScoped<RoboChemist.TemplateService.Service.Interfaces.IStorageService, RoboChemist.TemplateService.Service.Implements.StorageService>();

var accountId = builder.Configuration["CLOUDFLARE_R2_ACCOUNT_ID"];
var accessKeyId = builder.Configuration["CLOUDFLARE_R2_ACCESS_KEY_ID"];
var secretAccessKey = builder.Configuration["CLOUDFLARE_R2_SECRET_ACCESS_KEY"];
var bucketName = builder.Configuration["CLOUDFLARE_R2_BUCKET_NAME"];

Console.WriteLine("--- DEBUG R2 VALUES ---");
Console.WriteLine($"AccountId (Loaded): {accountId?.Substring(0, 4)}...");
Console.WriteLine($"AccessKeyId (Loaded): {accessKeyId?.Substring(0, 4)}...");
Console.WriteLine($"BucketName (Loaded): {bucketName}");

if (string.IsNullOrEmpty(accountId))
    throw new InvalidOperationException("LỖI CẤU HÌNH: Không tìm thấy 'CLOUDFLARE_R2_ACCOUNT_ID'");
if (string.IsNullOrEmpty(accessKeyId))
    throw new InvalidOperationException("LỖI CẤU HÌNH: Không tìm thấy 'CLOUDFLARE_R2_ACCESS_KEY_ID'");
if (string.IsNullOrEmpty(secretAccessKey))
    throw new InvalidOperationException("LỖI CẤU HÌNH: Không tìm thấy 'CLOUDFLARE_R2_SECRET_ACCESS_KEY'");
if (string.IsNullOrEmpty(bucketName))
    throw new InvalidOperationException("LỖI CẤU HÌNH: Không tìm thấy 'CLOUDFLARE_R2_BUCKET_NAME'");

var credentials = new BasicAWSCredentials(accessKeyId, secretAccessKey);

var s3Config = new AmazonS3Config
{
    ServiceURL = $"https://{accountId}.r2.cloudflarestorage.com",
    ForcePathStyle = true
};
Console.WriteLine($"DEBUG - R2 Endpoint: {s3Config.ServiceURL}"); 

// 3. Đăng ký IAmazonS3 làm singleton
builder.Services.AddSingleton<IAmazonS3>(new AmazonS3Client(credentials, s3Config));

var app = builder.Build();

// ============= MIDDLEWARE PIPELINE =============
// Order matters! Global Exception Handler should be FIRST

// 1. Global Exception Handler (catches all unhandled exceptions)
app.UseGlobalExceptionHandler();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Template Service API V1");
    c.RoutePrefix = "swagger";
});

// 3. Enable CORS
app.UseCors("AllowFrontend");

// 4. HTTPS Redirection (only in Production to avoid warnings in Development)
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// 5. Authorization
app.UseAuthorization();
app.MapControllers();

// XÓA BẤT KỲ CODE "CHECK BUCKET" NÀO KHỎI ĐÂY

Console.WriteLine("Swagger UI available at: http://localhost:5000/swagger");
Console.WriteLine("Application is starting...");

app.Run();


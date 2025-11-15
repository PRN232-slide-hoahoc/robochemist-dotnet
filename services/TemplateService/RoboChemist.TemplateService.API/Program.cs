using System.Reflection;
using System.Text;
using Amazon.Runtime;
using Amazon.S3;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RoboChemist.TemplateService.Model.Data;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel for large file uploads
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 100 * 1024 * 1024; // 100 MB
});

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

// JWT Authentication Configuration - ĐỌC TỪ .ENV
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") 
    ?? throw new Exception("JWT_SECRET not found in .env!");
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") 
    ?? throw new Exception("JWT_ISSUER not found in .env!");
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") 
    ?? throw new Exception("JWT_AUDIENCE not found in .env!");

Console.WriteLine($"[JWT] Secret loaded: {jwtSecret.Substring(0, 10)}...");
Console.WriteLine($"[JWT] Issuer: {jwtIssuer}");
Console.WriteLine($"[JWT] Audience: {jwtAudience}");

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

    // Logging cho development
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"[JWT] Authentication failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine($"[JWT] Token validated for user: {context.Principal?.Identity?.Name}");
            return Task.CompletedTask;
        },
        OnMessageReceived = context =>
        {
            var token = context.Token;
            if (!string.IsNullOrEmpty(token))
            {
                Console.WriteLine($"[JWT] Token received: {token.Substring(0, Math.Min(20, token.Length))}...");
            }
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            Console.WriteLine($"[JWT] Challenge triggered: {context.Error}, {context.ErrorDescription}");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// Add HttpContextAccessor for accessing user context in services
builder.Services.AddHttpContextAccessor();

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

// Add Controllers
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Template Service API",
        Version = "v1",
        Description = "API quản lý templates - Upload, Download, và quản lý file PowerPoint"
    });

    // Thêm JWT Security Definition vào Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n" +
                      "Nhập 'Bearer' [space] và sau đó nhập token của bạn.\r\n\r\n" +
                      "Ví dụ: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...\"",
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
            Array.Empty<string>()
        }
    });

    // Enable XML comments for Swagger documentation
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Configure file upload support
    c.MapType<IFormFile>(() => new OpenApiSchema
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

// Register DbContext as DbContext for Generic Repository pattern
builder.Services.AddScoped<Microsoft.EntityFrameworkCore.DbContext>(provider => provider.GetRequiredService<AppDbContext>());

// Register Unit of Work (repositories are created inside UnitOfWork)
builder.Services.AddScoped<RoboChemist.TemplateService.Repository.Interfaces.IUnitOfWork, RoboChemist.TemplateService.Repository.Implements.UnitOfWork>();

// HTTP Client Factory for AuthService
builder.Services.AddHttpClient("ApiGateway", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:ApiGateway:BaseUrl"] ?? "https://localhost:5001");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Register Services
builder.Services.AddScoped<RoboChemist.TemplateService.Service.Interfaces.ITemplateService, RoboChemist.TemplateService.Service.Implements.TemplateService>();
builder.Services.AddScoped<RoboChemist.TemplateService.Service.Interfaces.IStorageService, RoboChemist.TemplateService.Service.Implements.StorageService>();
builder.Services.AddScoped<RoboChemist.TemplateService.Service.Interfaces.IOrderService, RoboChemist.TemplateService.Service.Implements.OrderService>();
builder.Services.AddScoped<RoboChemist.TemplateService.Service.Interfaces.IUserTemplateService, RoboChemist.TemplateService.Service.Implements.UserTemplateService>();
builder.Services.AddScoped<RoboChemist.TemplateService.Service.HttpClients.IAuthServiceClient, RoboChemist.TemplateService.Service.HttpClients.AuthServiceClient>();

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();


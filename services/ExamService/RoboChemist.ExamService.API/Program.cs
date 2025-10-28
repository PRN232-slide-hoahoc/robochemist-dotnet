using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using RoboChemist.ExamService.Model.Data;
using RoboChemist.ExamService.Service.Interfaces;
using RoboChemist.ExamService.Service.Implements;
using RoboChemist.ExamService.Service.HttpClients;
using RoboChemist.ExamService.Repository.Interfaces;
using RoboChemist.ExamService.Repository.Implements;

var builder = WebApplication.CreateBuilder(args);

// Load .env file from solution root
var solutionRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".."));
var envPath = Path.Combine(solutionRoot, ".env");
Console.WriteLine($"[DEBUG] Looking for .env at: {envPath}");
Console.WriteLine($"[DEBUG] .env exists: {File.Exists(envPath)}");
DotNetEnv.Env.Load(envPath);
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddHttpContextAccessor(); // Cho phép services truy cập HttpContext
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// JWT Authentication Configuration - ĐỌC TỪ .ENV
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
        }
    };
});

builder.Services.AddAuthorization();

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
});

// Microservices: HTTP Client for API Gateway
// Chỉ cần 1 HttpClient trỏ đến Gateway, tất cả services đều gọi qua đây
builder.Services.AddHttpClient("ApiGateway", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:ApiGateway:BaseUrl"] ?? "https://localhost:5001");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Database & Repositories
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(Environment.GetEnvironmentVariable("EXAM_DB")));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// HttpClients - Gọi các service khác qua ApiGateway
builder.Services.AddScoped<ISlidesServiceHttpClient, SlidesServiceHttpClient>();

// Services
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IMatrixService, MatrixService>();
builder.Services.AddScoped<IExamService, ExamService>();

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

Console.WriteLine($"[DEBUG] EXAM_DB: {Environment.GetEnvironmentVariable("EXAM_DB")}");

app.Run();

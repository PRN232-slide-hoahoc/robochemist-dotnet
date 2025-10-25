using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;
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
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
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

    // Enable XML comments for Swagger documentation
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// ============================================
// Microservices: HTTP Client for API Gateway
// ============================================
// Chỉ cần 1 HttpClient trỏ đến Gateway, tất cả services đều gọi qua đây
builder.Services.AddHttpClient("ApiGateway", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:ApiGateway:BaseUrl"] ?? "https://localhost:5001");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Hoặc nếu muốn inject IHttpClientFactory trong service:
// var httpClient = _httpClientFactory.CreateClient("ApiGateway");
// await httpClient.GetAsync("/slides/v1/Topic/123");  // Slides Service
// await httpClient.GetAsync("/wallet/Wallet/user/456"); // Wallet Service
// await httpClient.GetAsync("/auth/User/validate");     // Auth Service

// ============================================
// Database & Repositories
// ============================================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(Environment.GetEnvironmentVariable("EXAM_DB")));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ============================================
// Business Services
// ============================================
builder.Services.AddScoped<IQuestionService, QuestionService>();
// TODO: Register other services as needed
// builder.Services.AddScoped<IMatrixService, MatrixService>();
// builder.Services.AddScoped<IExamService, ExamService>();

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

Console.WriteLine($"[DEBUG] EXAM_DB: {Environment.GetEnvironmentVariable("EXAM_DB")}");

app.Run();

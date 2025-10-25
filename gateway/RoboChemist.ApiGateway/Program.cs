using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MMLib.SwaggerForOcelot.DependencyInjection;
using MMLib.SwaggerForOcelot;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Load Ocelot configuration - environment-specific first, then fallback to main
builder.Configuration
    .AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Add Ocelot
builder.Services.AddOcelot(builder.Configuration);

// Add SwaggerForOcelot to aggregate downstream Swagger docs
builder.Services.AddSwaggerForOcelot(builder.Configuration);

// Add Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Keep a minimal Gateway swagger (useful to document gateway-specific endpoints)
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("gateway", new OpenApiInfo
    {
        Title = "RoboChemist API Gateway",
        Version = "gateway",
        Description = "API Gateway swagger (contains Gateway-specific endpoints)."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header. Enter: Bearer {your_token}"
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

// JWT Authentication - Same configuration as AuthService
var secretKey = "YourSuperSecretKeyMinimum32CharactersLong!@#$%^&*()";
var issuer = "RoboChemist.AuthService";
var audience = "RoboChemist.Client";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure middleware
app.UseCors("AllowAll");

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// In development: use SwaggerForOcelot UI (aggregated dropdown) and also Gateway's own swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // gateway-specific swagger at /swagger/gateway/swagger.json

    // SwaggerForOcelot UI aggregates downstream swagger definitions and provides "Select a definition" dropdown
    app.UseSwaggerForOcelotUI(options =>
    {
        options.PathToSwaggerGenerator = "/swagger/docs"; // default path used by SwaggerForOcelot
    });
}

// Use Ocelot at the end
await app.UseOcelot();

app.Run();

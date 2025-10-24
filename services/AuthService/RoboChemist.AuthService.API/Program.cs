using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using RoboChemist.AuthService.Model.Data;
using RoboChemist.AuthService.Repository;
using RoboChemist.AuthService.Services;

namespace RoboChemist.AuthService.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);



            var connectionString = "Host=ep-snowy-cell-a19ensgj-pooler.ap-southeast-1.aws.neon.tech;Port=5432;Database=robochemist_authservice;Username=neondb_owner;Password=npg_CeHyrLVF3pb6;SSL Mode=Require;Trust Server Certificate=true";

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));

            // Add Repository & Service
            builder.Services.AddScoped<UserRepository>();
            builder.Services.AddScoped<UserService>();

            // Cấu hình JWT Authentication - Hardcode (không cần appsettings)
            var secretKey = "YourSuperSecretKeyMinimum32CharactersLong!@#$%^&*()";
            var issuer = "RoboChemist.AuthService";
            var audience = "RoboChemist.Client";

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
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

            builder.Services.AddAuthorization();

            // Add Controllers
            builder.Services.AddControllers();

            // Cấu hình Swagger với JWT
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "RoboChemist Auth API",
                    Version = "v1"
                });

                // Thêm JWT Authentication vào Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
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
            });

            // Cấu hình CORS (nếu cần)
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

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // QUAN TRỌNG: Thứ tự phải đúng
            app.UseCors("AllowAll");

            app.UseAuthentication(); // Phải đặt trước UseAuthorization
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
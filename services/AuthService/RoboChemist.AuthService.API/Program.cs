using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using DotNetEnv;
using RoboChemist.AuthService.Model.Data;
using RoboChemist.AuthService.API;
using RoboChemist.AuthService.Repository;
using RoboChemist.AuthService.Services;

namespace RoboChemist.AuthService.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ✅ Load .env từ thư mục gốc solution
            var solutionRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".."));
            var envPath = Path.Combine(solutionRoot, ".env");
            Console.WriteLine($"[DEBUG] Looking for .env at: {envPath}");
            Console.WriteLine($"[DEBUG] .env exists: {File.Exists(envPath)}");
            DotNetEnv.Env.Load(envPath);
            builder.Configuration.AddEnvironmentVariables();

            // ✅ Lấy connection string từ biến môi trường AUTH_DB
            var connectionString = Environment.GetEnvironmentVariable("USER_DB");
            Console.WriteLine($"[DEBUG] AUTH_DB: {connectionString}");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception("❌ Không tìm thấy connection string! Hãy set biến AUTH_DB trong file .env hoặc thêm vào appsettings.json");
            }

            // ✅ Add DbContext
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));

            // ---------------------------------------------------------
            // 🔹 3. Cấu hình Repository & Services
            // ---------------------------------------------------------
            builder.Services.AddScoped<UserRepository>();
            builder.Services.AddScoped<UserService>();

            // ---------------------------------------------------------
            // 🔹 4. Load JWT settings từ appsettings.json
            // ---------------------------------------------------------
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
            var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();

            if (jwtSettings == null || string.IsNullOrEmpty(jwtSettings.Key))
            {
                throw new Exception("❌ Không tìm thấy thông tin JWT trong appsettings.json!");
            }

            // ---------------------------------------------------------
            // 🔹 5. Cấu hình JWT Authentication (không hardcode)
            // ---------------------------------------------------------
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key));

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
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = key,
                    ClockSkew = TimeSpan.Zero
                };
            });

            builder.Services.AddAuthorization();

            // ---------------------------------------------------------
            // 🔹 6. Add Controllers, Swagger & CORS
            // ---------------------------------------------------------
            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "RoboChemist Auth API",
                    Version = "v1",
                    Description = "Authentication and User Management Service for RoboChemist"
                });

                // Swagger JWT Authorization
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {your token}'",
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

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            // ---------------------------------------------------------
            // 🔹 7. Build app
            // ---------------------------------------------------------
            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}

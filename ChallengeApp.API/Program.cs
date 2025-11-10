using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ChallengeApp.API.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Mvc;

namespace ChallengeApp.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // OpenAPI
            builder.Services.AddOpenApi();

            // Database
            builder.Services.AddDbContext<ChallengeAppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Controllers + Validation
            builder.Services.AddControllers()
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.InvalidModelStateResponseFactory = context =>
                    {
                        var errors = context.ModelState
                            .Where(e => e.Value.Errors.Count > 0)
                            .Select(e => new { Field = e.Key, Error = e.Value.Errors.First().ErrorMessage });

                        return new BadRequestObjectResult(new { message = "Validation failed", errors });
                    };
                });

            // API Versioning
            builder.Services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
            });

            // Rate Limiting
            builder.Services.AddMemoryCache();
            builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
            builder.Services.AddInMemoryRateLimiting();
            builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

            // JWT Auth
            var jwtKey = builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT secret is not configured");
            var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap["emailaddress"] = "email";
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap[ClaimTypes.Role] = ClaimTypes.Role;

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = "ChallengeApp",
                        ValidAudience = "ChallengeAppUsers",
                        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                        NameClaimType = "email",
                        RoleClaimType = ClaimTypes.Role
                    };
                });

            builder.Services.AddAuthorization();

            // CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .WithExposedHeaders("Content-Disposition");
                });
            });

            var app = builder.Build();
            app.MapOpenApi();            
            app.UseHttpsRedirection();
            app.UseCors("AllowAll");
            app.UseIpRateLimiting();
            app.UseMiddleware<ErrorHandlingMiddleware>();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}

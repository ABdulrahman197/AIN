using AIN.Api.endpoints;
using AIN.Api.Endpoints;
using AIN.Application.Interfaces.IRepos;
using AIN.Application.Interfaces.IServices;
using AIN.Application.Services;
using AIN.Core.Entites;
using AIN.Core.Enums;
using AIN.Infrastructrue.Context;
using AIN.Infrastructrue.Repos;
using AIN.Infrastructure.Services;
using BCrypt.Net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Org.BouncyCastle.Crypto.Generators;
using StackExchange.Redis;
using System.Text;
using System.Threading.RateLimiting;
var builder = WebApplication.CreateBuilder(args);

// -----------------------------
// Database
// -----------------------------
builder.Services.AddDbContext<AinDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString);
});

//// -----------------------------
//// Redis Cache
//// -----------------------------
//builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
//{
//    var connection = builder.Configuration.GetConnectionString("Redis");
//    return ConnectionMultiplexer.Connect(connection);
//});

//builder.Services.AddScoped<ICache, CacheAttribute>(); 

// -----------------------------
// Dependency Injection (Repos + Services)
// -----------------------------
builder.Services.AddScoped<IReportRepo, ReportRepo>();
builder.Services.AddScoped<IUserRepo, UserRepo>();
builder.Services.AddScoped<IAuthorityRepo, AuthorityRepo>();
builder.Services.AddScoped<ILikeRepo, LikeRepo>();
builder.Services.AddScoped<ICommentRepo, CommentRepo>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IRoutingService, RoutingService>();
builder.Services.AddScoped<ITrustPointsService, TrustPointsService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ILikeService, LikeService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IAdminService, AdminService>();
// Fixed: Removed duplicate IEmailService registration
builder.Services.AddScoped<IEmailService, SmtpEmailService>();

// -----------------------------
// CORS (for React dev origin)
// -----------------------------
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
	options.AddPolicy("Frontend", policy =>
	{
		if (allowedOrigins.Length > 0)
		{
			policy.WithOrigins(allowedOrigins)
				.AllowAnyHeader()
				.AllowAnyMethod()
				.AllowCredentials();
		}
		else
		{
			policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
		}
	});
});

// -----------------------------
// Controllers + MVC Views
// -----------------------------
builder.Services.AddControllersWithViews();

// -----------------------------
// Authentication (JWT)
// -----------------------------
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection.GetValue<string>("Key") ?? "";
var issuer = jwtSection.GetValue<string>("Issuer") ?? "";
var audience = jwtSection.GetValue<string>("Audience") ?? "";

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
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
    };
});

// -----------------------------
// Authorization (Role-based)
// -----------------------------
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireAuthority", policy => policy.RequireRole("Authority"));
    options.AddPolicy("RequireUser", policy => policy.RequireRole("User"));
});

// -----------------------------
// Swagger
// -----------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by your token"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            Array.Empty<string>()
        }
    });
});

// -----------------------------
// Rate Limiting
// -----------------------------
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 1000,
            Window = TimeSpan.FromMinutes(1),
        });
    });

    options.RejectionStatusCode = 429;
    options.OnRejected = async (ctx, token) =>
    {
        ctx.HttpContext.Response.ContentType = "application/json";
        await ctx.HttpContext.Response.WriteAsync(
            "{\"error\": \"Too many requests. Please try again later.\"}", token);
    };
});

// -----------------------------
// Build App
// -----------------------------
var app = builder.Build();

// -----------------------------
// Seed
//trial for seeding
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AinDbContext>();
    if (!await db.Authorities.AnyAsync())
    {
        db.Authorities.AddRange(
            new AIN.Core.Entites.Authority { Id = Guid.NewGuid(), Name = "Police", Department = "Security", ContactEmail = "police@example.com", ContactPhone = "+100000000" },
            new AIN.Core.Entites.Authority { Id = Guid.NewGuid(), Name = "Ambulance", Department = "Public Safety", ContactEmail = "ambulance@example.com", ContactPhone = "+100000001" },
            new AIN.Core.Entites.Authority { Id = Guid.NewGuid(), Name = "Traffic Department", Department = "Traffic", ContactEmail = "traffic@example.com", ContactPhone = "+100000002" },
            new AIN.Core.Entites.Authority { Id = Guid.NewGuid(), Name = "Municipality", Department = "Environment", ContactEmail = "municipality@example.com", ContactPhone = "+100000003" },
            new AIN.Core.Entites.Authority { Id = Guid.NewGuid(), Name = "General Authority", Department = "General", ContactEmail = "general@example.com", ContactPhone = "+100000004" }
        );
        await db.SaveChangesAsync();
    }
//db.Set<UserAccount>().Add(new UserAccount { Id = Guid.NewGuid(), DisplayName = "Author", Email = "Authority@ain.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Authority@123"), Role = enums.UserRole.Authority , IsEmailConfirmed = true, TrustPoints = 50, Badge = enums.TrustBadge.Trusted , });
}
// -----------------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userRepo = services.GetRequiredService<IUserRepo>();
    var config = services.GetRequiredService<IConfiguration>();

    await DbSeeder.SeedAdminAsync(userRepo, config);
}


// -----------------------------
// Auto Migration
// -----------------------------
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AinDbContext>();
    var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();

    try
    {
        await dbContext.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        var logger = loggerFactory.CreateLogger<Program>();
        logger.LogError(ex, "An error occurred during migration.");
    }
}

// -----------------------------
// Middleware
// -----------------------------

    app.UseSwagger();
    app.UseSwaggerUI();



app.UseHttpsRedirection();

// CORS must be before auth
app.UseCors("Frontend");

// Promote JWT from cookie if present
app.Use(async (context, next) =>
{
    if (!context.Request.Headers.ContainsKey("Authorization") && context.Request.Cookies.TryGetValue("token", out var token) && !string.IsNullOrEmpty(token))
    {
        context.Request.Headers.Append("Authorization", $"Bearer {token}");
    }
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

// Add static files middleware for serving uploaded files
app.UseStaticFiles();

// -----------------------------
// Endpoints (API + MVC)
// -----------------------------
app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// -----------------------------
// SPA fallback for built React app under /app
// -----------------------------
app.MapFallbackToFile("/app/{*path}", "app/index.html");

// Fixed: Removed duplicate /api/users prefix
// Minimal API groups removed in favor of MVC ApiControllers

app.Run();

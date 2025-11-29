using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SchoolKeeper.Abstractions.Interfaces.Repository;
using SchoolKeeper.Authorization;
using SchoolKeeper.Middleware;
using SchoolKeeper.Models.Enums;
using SchoolKeeper.Services;
using Sonar.Infrastructure.Repository;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<SchoolKeeperDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection") ??
                      throw new InvalidOperationException("Connection string 'DefaultConnection' not found.")));

builder.Services.AddControllers();
builder.Services.AddRazorPages();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "������� JWT ����� � �������: Bearer {your token}"
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
            new string[] {}
        }
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// JWT Authentication - только для API
// Для Razor Pages авторизация обрабатывается вручную через UserService
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
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
    
    // Для Razor Pages читаем токен из cookie
    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Для API используем токен из заголовка Authorization
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                }
            }
            // Для Razor Pages используем токен из cookie
            else if (context.Request.Cookies.TryGetValue("authToken", out var cookieToken))
            {
                context.Token = cookieToken;
            }
            
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            // Для API возвращаем 401
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.HandleResponse();
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                return Task.CompletedTask;
            }
            // Для Razor Pages очищаем cookie при 401 и перенаправляем на Login
            else
            {
                // Очищаем cookie авторизации
                context.Response.Cookies.Delete("authToken");
                context.Response.Cookies.Delete("isImpersonating");
                context.Response.Cookies.Delete("originalAdminId");
                
                // Перенаправляем на страницу логина
                context.Response.Redirect("/Login");
                context.HandleResponse();
                return Task.CompletedTask;
            }
        }
    };
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // 100 MB
});

// Authorization Policies
builder.Services.AddAuthorization(options =>
{
    Policies.ConfigurePolicies(options);
});

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IDataFilterService, DataFilterService>();
builder.Services.AddScoped<SchoolKeeper.Services.IReportGenerationService, SchoolKeeper.Services.ReportGenerationService>();


WebApplication app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var seeder = new DbSeeder(scope.ServiceProvider.GetRequiredService<SchoolKeeperDbContext>());
    await seeder.SeedAsync();
}

// Exception handling middleware (should be registered early)
app.UseMiddleware<ExceptionMiddleware>();

// Swagger middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SchoolKeeper API v1");
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // ВАЖНО: для обслуживания CSS, JS и других статических файлов
app.UseRouting();
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();
// Middleware для очистки cookie при 401 ошибке (должен быть после UseAuthorization)
app.UseMiddleware<CookieCleanupMiddleware>();
app.MapRazorPages();
app.MapControllers();
app.Run();

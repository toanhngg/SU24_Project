using BusinessObject.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Cấu hình DbContext
        builder.Services.AddDbContext<PizzaLabContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        // Cấu hình Authentication với JWT
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
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
            };

            options.Events = new JwtBearerEvents
            {
                // Thêm event lấy token từ cookie
                OnMessageReceived = context =>
                {
                    var token = context.Request.Cookies["authToken"]; // Lấy token từ cookie
                    if (!string.IsNullOrEmpty(token))
                    {
                        context.Token = token; // Thiết lập token từ cookie
                    }
                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = context =>
                {
                    Console.WriteLine("Authentication failed: " + context.Exception.Message);
                    return Task.CompletedTask;
                }
            };
        });

        // Cấu hình CORS để hỗ trợ Cross-Origin Requests
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowSpecificOrigins",
                policyBuilder =>
                {
                    policyBuilder.WithOrigins("https://localhost:7164") // URL của client
                           .AllowAnyHeader()
                           .AllowAnyMethod()
                           .AllowCredentials(); // Cho phép gửi cookie
                });
        });

        // Cấu hình MVC và Swagger cho API
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Swagger eShop Solution", Version = "v1" });
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\n
                                Enter 'Bearer' [space] and then your token in the text input below.
                                \r\n\r\nExample: 'Bearer 12345abcdef'",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });
        });

        var app = builder.Build();

        // Sử dụng CORS
        app.UseCors("AllowSpecificOrigins");

        // Middleware để chuyển hướng https
        app.UseHttpsRedirection();

        // Middleware để cấu hình Authentication và Authorization
        app.UseAuthentication();
        app.UseAuthorization();

        // Middleware để lấy JWT từ cookie và gắn vào header Authorization
        app.Use(async (context, next) =>
        {
            var token = context.Request.Cookies["authToken"]; // Lấy JWT từ cookie

            if (!string.IsNullOrEmpty(token))
            {
                context.Request.Headers.Add("Authorization", $"Bearer {token}"); // Đính kèm JWT vào header Authorization
            }

            await next();
        });

        // Cấu hình Swagger cho môi trường phát triển
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Cấu hình các endpoint
        app.MapControllers();

        // Chạy ứng dụng
        app.Run();
    }
}

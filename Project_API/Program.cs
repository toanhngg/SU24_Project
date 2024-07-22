using BusinessObject.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Project_API.Extensions;
using Project_API.Service;
using System.Text;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        //var rabbitMqConfig = builder.Configuration.GetSection("RabbitMQ");
        //string hostName = rabbitMqConfig["HostName"];
        //string userName = rabbitMqConfig["UserName"];
        //string password = rabbitMqConfig["Password"];

        //// Thêm RabbitMQService vào DI container
        //builder.Services.AddRabbitMQ(hostName, userName, password);
        //builder.Services.AddSingleton<RabbitMQService>();

        // Cấu hình DbContext
        builder.Services.AddDbContext<PizzaLabContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        // Cấu hình Authentication
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; // Thay đổi thành JwtBearerDefaults.AuthenticationScheme nếu bạn muốn JwtBearer làm scheme mặc định
        })
        .AddCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.Name = "authToken";
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
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
                ValidAudience = builder.Configuration["Jwt:Issuer"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
            };
        });

        // Cấu hình CORS
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins("https://localhost:7164") // Thay thế bằng URL của bạn
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials(); // Cho phép gửi cookie
            });
        });

        // Cấu hình MVC và Swagger
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
        app.UseCors();

        // Middleware để chuyển hướng https
        app.UseHttpsRedirection();

        // Middleware để cấu hình Authentication và Authorization
        app.UseAuthentication();
        app.UseAuthorization();

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

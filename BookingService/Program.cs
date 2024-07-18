using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Đọc cấu hình từ appsettings.json
            var configuration = builder.Configuration;
            builder.Services.AddCors();

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Đăng ký PizzaLabContext với DI container
            builder.Services.AddDbContext<PizzaLabContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Lấy cấu hình RabbitMQ từ appsettings.json
            var rabbitMQConfig = configuration.GetSection("RabbitMQ");
            var hostName = rabbitMQConfig["HostName"];
            var userName = rabbitMQConfig["UserName"];
            var password = rabbitMQConfig["Password"];

            // Thêm các dịch vụ RabbitMQ
            builder.Services.AddRabbitMQ(hostName, userName, password);

            var app = builder.Build();
            app.UseCors(policy =>
                              policy.AllowAnyOrigin()
                              .AllowAnyHeader()
                              .AllowAnyMethod());
            // Cấu hình pipeline xử lý HTTP request
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();

        }
    }
}
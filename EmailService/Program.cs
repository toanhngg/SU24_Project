using EmailService.Extensions;
using EmailService.IRepository;
using EmailService.Services;
using EmailService.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EmailService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
            builder.Services.AddTransient<IEmailSender, EmailSender>();
            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Retrieve RabbitMQ configuration from appsettings.json
            var configuration = builder.Configuration.GetSection("RabbitMQ");
            var hostName = configuration["HostName"];
            var userName = configuration["UserName"];
            var password = configuration["Password"];

            // Add RabbitMQ services
            builder.Services.AddRabbitMQ(hostName, userName, password);
            builder.Services.AddHostedService<EmailWorker>();

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

            app.Run();
        }
    }
}

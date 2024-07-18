using EmailService.IRepository;
using EmailService.Models;
using EmailService.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public class EmailWorker : BackgroundService
{
    private readonly IConnection _rabbitConnection;
    private readonly IEmailSender _emailSender;
    private IModel _channel;

    public EmailWorker(IConnection rabbitConnection, IEmailSender emailSender)
    {
        _rabbitConnection = rabbitConnection;
        _emailSender = emailSender;
        InitializeRabbitMQListener();
    }

    private void InitializeRabbitMQListener()
    {
        _channel = _rabbitConnection.CreateModel();
        _channel.ExchangeDeclare(exchange: "booking_exchange", type: ExchangeType.Direct);
        _channel.QueueDeclare(queue: "booking_queue", durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueBind(queue: "booking_queue", exchange: "booking_exchange", routingKey: "booking");
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var bookingMessage = JsonSerializer.Deserialize<BookingMessage>(message);

            // Process the message
            await _emailSender.SendEmailAsync(bookingMessage.Email, bookingMessage.Subject, bookingMessage.Message);

            _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
        };

        _channel.BasicConsume(queue: "booking_queue", autoAck: false, consumer: consumer);

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel.Close();
        _rabbitConnection.Close();
        base.Dispose();
    }
}
using BookingService.DTO;
using BusinessObject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using System.Net.Http.Json; // Import this namespace
using System.Net.Http.Headers;

namespace BookingService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly HttpClient client = null;
        private readonly PizzaLabContext _context;
        private readonly IConnection _rabbitConnection;

        public BookingController(PizzaLabContext context, IConnection rabbitConnection)
        {
            _context = context;
            _rabbitConnection = rabbitConnection;
            var content = new MediaTypeWithQualityHeaderValue("application/json");

        }
        [HttpPost("book")]
        public async Task<IActionResult> BookTable([FromBody] BookingDTO request)
        {
            try
            {
                // 1. Call Customer Service to save customer
                int customerResponse = await SaveCustomer(request.Customer);
                if (TimeSpan.TryParse(request.Time, out TimeSpan timeSpan))
                {
                    var bookingId = await SaveBooking(new Booking
                    {
                        CustomerId = customerResponse,
                        Date = request.Date,
                        Time = timeSpan,
                        NumberOfPeople = request.NumberOfPeople,
                        Note = request.Note
                    });
                    PublishBookingEvent(request.Customer, bookingId);
                }

                // 3. Publish booking event to RabbitMQ
                // 2. Publish booking event to RabbitMQ
                return Ok("Booking successfully confirmed.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        private async Task<int> SaveBooking(Booking booking)
        {
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
            return booking.Id;
        }
        private async Task<int> SaveCustomer(CustomerDTO customer)
        {
            using (var client = new HttpClient())
            {
                var response = await client.PostAsJsonAsync("https://localhost:7135/api/Customer", customer);
                response.EnsureSuccessStatusCode(); // Ensure success before reading content

                // Read response content as JSON and specify the type
                var content = await response.Content.ReadFromJsonAsync<Customer>();

                // Return the id of the customer (assuming CustomerDTO has an Id property)
                return content.Id;
            }
        }


        private void PublishBookingEvent(CustomerDTO customer, int bookingId)
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost", // RabbitMQ host
                UserName = "agent",     // RabbitMQ username
                Password = "agent"      // RabbitMQ password
            };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                // Declare an exchange
                channel.ExchangeDeclare(exchange: "booking_exchange", type: ExchangeType.Direct);

                // Serialize message
                var bookingMessage = new EmailRequest
                {
                    CustomerName = customer.Name,
                    Email = customer.Email,
                    Subject = "Booking Confirmation",
                    Message = $"Dear {customer.Name}, your booking has been confirmed."
                };
                var messageBody = JsonSerializer.Serialize(bookingMessage);
                var body = Encoding.UTF8.GetBytes(messageBody);

                // Publish message to RabbitMQ
                channel.BasicPublish(exchange: "booking_exchange",
                                     routingKey: "booking",
                                     basicProperties: null,
                                     body: body);
            }
        }
    }
    //[HttpPost]
    //    public IActionResult BookTable([FromBody] BookingDTO request)
    //    {
    //        var factory = new ConnectionFactory() { HostName = "localhost" };
    //        using (var connection = factory.CreateConnection())
    //        using (var channel = connection.CreateModel())
    //        {
    //            channel.QueueDeclare(queue: "booking_queue",
    //                                 durable: false,
    //                                 exclusive: false,
    //                                 autoDelete: false,
    //                                 arguments: null);

    //    string message = $"{request.Date},{request.Time},{request.Customer.Name},{request.NumberOfPeople},{request.Note}";
    //            var body = Encoding.UTF8.GetBytes(message);

    //            channel.BasicPublish(exchange: "",
    //                                 routingKey: "booking_queue",
    //                                 basicProperties: null,
    //                                 body: body);
    //            Console.WriteLine(" [x] Sent {0}", message);
    //        }

    //        return Ok(new { Status = "Table booked successfully!" });
    //    }
    //}
}

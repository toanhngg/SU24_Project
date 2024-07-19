using BookingService.DTO;
using BusinessObject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using System.Net.Http.Json; // Import this namespace
using System.Net.Http.Headers;
using System.Numerics;
using System.Reflection.Metadata;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;

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
                int customerResponse = await SaveCustomer(request.Customer);
                if (TimeSpan.TryParse(request.Time, out TimeSpan timeSpan))
                {
                    if (request.Date < DateTime.Now)
                    {
                        return BadRequest(new { title = "Không được đặt bàn quá khứ." });
                    }


                    TimeSpan bookingDuration = TimeSpan.FromHours(2);

                    // Tính DateStart và DateCheckOut cho booking mới
                    DateTime dateStart = request.Date.Value + timeSpan;
                    DateTime dateCheckOut = dateStart + bookingDuration;

                    // Kiểm tra xem có booking nào trùng lặp không
                    var existingBookings = await _context.Bookings
                        .Where(b => b.BookingTable == request.BookingTable &&
                                    ((b.DateStart < dateCheckOut && b.DateCheckOut > dateStart) || // A < C < B
                                     (dateStart < b.DateStart && (dateCheckOut - dateStart) < bookingDuration) || // C < A < 2h
                                     dateStart > b.DateCheckOut)) // C > B
                        .ToListAsync();

                    if (existingBookings.Any())
                    {
                        return BadRequest(new { title = "Hết bàn vào thời gian này." });
                    }

                    // Lưu booking nếu không có trùng lặp
                    var bookingId = await SaveBooking(new Booking
                    {
                        CustomerId = customerResponse,
                        Date = request.Date,
                        Time = timeSpan,
                        NumberOfPeople = request.NumberOfPeople,
                        Note = request.Note,
                        DateBooking = DateTime.Now

                    });
                    PublishBookingEvent(request.Customer, bookingId);
                }
                return Ok(new { title = "Booking successfully confirmed." });

            }
            catch (Exception ex)
            {
                return BadRequest(new { title = $"Internal server error: {ex.Message}" });
            }
        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var listBooking = _context.Bookings.ToList();
                return Ok(listBooking);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("getDetailById/{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var detailBooking = _context.Bookings.ToList().Where(x => x.Id == id);
                return Ok(detailBooking);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("update/{id}")]
        public async Task<IActionResult> updateBooking([FromBody] BookingUpdateDTO req)
        {
            try
            {
                Booking detailBooking = _context.Bookings.ToList().Where(x => x.Id == req.Id).FirstOrDefault();
                detailBooking.Date = req.Date;
                detailBooking.Time = req.Time;
                detailBooking.NumberOfPeople = req.NumberOfPeople;
                detailBooking.Note = req.Note;
                detailBooking.DateBooking = req.DateBooking;
                detailBooking.DateStart = req.DateStart;
                detailBooking.DateCheckOut = req.DateCheckOut;
                detailBooking.BookingTable = req.BookingTable;
                detailBooking.IsCheck = req.IsCheck;

                return Ok(detailBooking);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
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

                var content = await response.Content.ReadFromJsonAsync<Customer>();

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
                channel.ExchangeDeclare(exchange: "booking_exchange", type: ExchangeType.Direct);

                var bookingMessage = new EmailRequest
                {
                    CustomerName = customer.Name,
                    Email = customer.Email,
                    Subject = "Booking Confirmation",
                    Message = $"Dear {customer.Name}, your booking has been confirmed."
                };
                var messageBody = JsonSerializer.Serialize(bookingMessage);
                var body = Encoding.UTF8.GetBytes(messageBody);

                channel.BasicPublish(exchange: "booking_exchange",
                                     routingKey: "booking",
                                     basicProperties: null,
                                     body: body);
            }
        }
    }
}

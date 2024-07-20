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
using Microsoft.AspNetCore.Authorization;
using System;

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
                if (request.Date == null || request.Time == null)
                {
                    return BadRequest(new { title = "Date and Time are required." });
                }

                if (!TimeSpan.TryParse(request.Time, out TimeSpan timeSpan))
                {
                    return BadRequest(new { title = "Invalid time format." });
                }

                if (request.Date < DateTime.Now.Date)
                {
                    return BadRequest(new { title = "Không được đặt bàn trong quá khứ." });
                }

                TimeSpan bookingDuration = TimeSpan.FromHours(2);
                DateTime dateStart = request.Date.Value.Date + timeSpan;
                DateTime dateCheckOut = dateStart + bookingDuration;

                // Lấy danh sách các bàn còn trống và tổng số chỗ trống
                var availableTables = await _context.Tables
                    .Where(t => !_context.Bookings
                        .Any(b => (b.DateStart < dateCheckOut && b.DateCheckOut > dateStart) ||
                                  (dateStart < b.DateStart && dateCheckOut > b.DateStart) ||
                                  (b.DateStart < dateCheckOut && dateStart < b.DateCheckOut)))
                    .ToListAsync();

                var totalCapacity = availableTables.Sum(t => t.NumberOfPeople);

                if (totalCapacity < request.NumberOfPeople)
                {
                    return BadRequest(new { title = "Hết bàn vào thời gian này." });
                }

                // Tạo booking mới
                var booking = new Booking
                {
                    CustomerId = await SaveCustomer(request.Customer),
                    Date = request.Date,
                    Time = timeSpan,
                    NumberOfPeople = request.NumberOfPeople,
                    Note = request.Note,
                    DateBooking = DateTime.Now,
                    DateStart = dateStart,
                    DateCheckOut = dateCheckOut,
                    Tables = new List<Table>() // Tạo danh sách các bàn được đặt
                };

                int remainingPeople = (int)request.NumberOfPeople;

                // Phân bổ số lượng người cho các bàn
                foreach (var table in availableTables)
                {
                    if (remainingPeople <= 0)
                        break;

                    int peopleForThisTable = Math.Min(remainingPeople, (int)table.NumberOfPeople);
                    booking.Tables.Add(table);
                    remainingPeople -= peopleForThisTable;
                }

                if (remainingPeople > 0)
                {
                    return BadRequest(new { title = "Không đủ bàn để phục vụ số lượng người yêu cầu." });
                }

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                PublishBookingEvent(request.Customer, booking.Id);

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
        public async Task<IActionResult> getDetailById(int id)
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
        [HttpPost("update/{id}")]
        public async Task<IActionResult> UpdateBooking([FromBody] BookingUpdateDTO req)
        {
            try
            {
                var emailClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email);
                if (emailClaim == null)
                {
                    return BadRequest(new { title = "Không tìm thấy email người dùng." });
                }

                var userEmail = emailClaim.Value.ToLower();
                var user = await _context.Users.FirstOrDefaultAsync(x => x.Email.ToLower() == userEmail);
                if (user == null)
                {
                    return BadRequest(new { title = "Người dùng không tồn tại." });
                }

                var detailBooking = await _context.Bookings
                    .Include(b => b.Tables) // Include related Tables for updating
                    .FirstOrDefaultAsync(x => x.Id == req.Id);
                if (detailBooking == null)
                {
                    return NotFound(new { title = "Không tìm thấy booking." });
                }

                if (req.Date == null || req.Time == null || !TimeSpan.TryParse(req.Time, out TimeSpan timeSpan))
                {
                    return BadRequest(new { title = "Thời gian không hợp lệ." });
                }

                // Tính DateStart và DateCheckOut cho booking mới
                DateTime dateStart = req.Date.Value + timeSpan;
                DateTime dateCheckOut = dateStart + TimeSpan.FromHours(2);

                // Lấy danh sách các bàn còn trống và tổng số chỗ trống
                var availableTables = await _context.Tables
                    .Where(t => !_context.Bookings
                        .Any(b => b.Id != req.Id &&
                                  ((b.DateStart < dateCheckOut && b.DateCheckOut > dateStart) || // A < C < B
                                   (dateStart < b.DateStart && dateCheckOut > b.DateStart) || // C < A < D
                                   (b.DateStart < dateCheckOut && dateStart < b.DateCheckOut)))) // A < D < B
                    .OrderByDescending(t => t.NumberOfPeople) // Sắp xếp bàn theo số lượng người (từ lớn đến nhỏ)
                    .ToListAsync();

                var totalCapacity = availableTables.Sum(t => t.NumberOfPeople);

                if (totalCapacity < req.NumberOfPeople)
                {
                    return BadRequest(new { title = "Hết bàn vào thời gian này." });
                }

                // Phân bổ số lượng người cho các bàn
                int remainingPeople = (int)req.NumberOfPeople;
                var newTables = new List<Table>();

                foreach (var table in availableTables)
                {
                    if (remainingPeople <= 0)
                        break;

                    int peopleForThisTable = Math.Min(remainingPeople, (int)table.NumberOfPeople);
                    newTables.Add(table);
                    remainingPeople -= peopleForThisTable;
                }

                if (remainingPeople > 0)
                {
                    return BadRequest(new { title = "Không đủ bàn để phục vụ số lượng người yêu cầu." });
                }

                // Cập nhật thông tin booking
                detailBooking.Date = req.Date.Value;
                detailBooking.Time = timeSpan;
                detailBooking.NumberOfPeople = req.NumberOfPeople;
                detailBooking.Note = req.Note;
                detailBooking.DateBooking = DateTime.Now;
                detailBooking.DateStart = dateStart;
                detailBooking.DateCheckOut = dateCheckOut;
                detailBooking.IsCheck = req.IsCheck;
                detailBooking.UserCheck = user.Id;

                // Xóa các liên kết cũ với các bàn
                var oldTables = detailBooking.Tables.ToList();
                detailBooking.Tables.Clear();

                // Thêm các liên kết mới với các bàn
                foreach (var table in newTables)
                {
                    detailBooking.Tables.Add(table);
                }

                _context.Bookings.Update(detailBooking);

                await _context.SaveChangesAsync();
                return Ok(detailBooking);
            }
            catch (Exception ex)
            {
                return BadRequest(new { title = $"Internal server error: {ex.Message}" });
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

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
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

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
                    return BadRequest(new { title = "Cannot book a table in the past." });
                }

                TimeSpan bookingDuration = TimeSpan.FromHours(2);
                DateTime dateStart = request.Date.Value.Date + timeSpan;
                DateTime dateCheckOut = dateStart + bookingDuration;

                DateTime now = DateTime.UtcNow; // Or DateTime.Now if you are using local time

                // Fetch tables and bookings for the requested date
                var tables = await _context.Tables.ToListAsync();
                var bookings = await _context.Bookings
                    .Where(b => b.Date.Value.Date == request.Date.Value.Date)
                    .Include(b => b.Tables) // Include related tables for each booking
                    .ToListAsync();

                // Filter out tables that are not available during the requested period
                var availableTables = tables
                    .Where(t => !bookings
                        .Any(b => b.Tables
                            .Any(tb => tb.Id == t.Id &&
                                (
                                    (b.DateStart.HasValue && b.DateCheckOut.HasValue &&
                                     b.DateStart.Value < dateCheckOut && b.DateCheckOut.Value > dateStart) ||
                                     (!b.DateStart.HasValue && !b.DateCheckOut.HasValue &&
                                     b.Date.Value.Date + b.Time + bookingDuration > now &&
                                     (b.Date.Value.Date + b.Time < dateCheckOut &&
                                      b.Date.Value.Date + b.Time + bookingDuration > dateStart))
                                )
                            )
                        )
                    )
                    .ToList();

                var totalCapacity = availableTables.Sum(t => t.NumberOfPeople);

                if (totalCapacity < request.NumberOfPeople)
                {
                    return BadRequest(new { title = "No available tables for the requested number of people." });
                }

                // Create a new booking
                var booking = new Booking
                {
                    CustomerId = await SaveCustomer(request.Customer),
                    Date = request.Date,
                    Time = timeSpan,
                    NumberOfPeople = request.NumberOfPeople,
                    Note = request.Note,
                    DateBooking = DateTime.Now,
                    Tables = new List<Table>() // List of booked tables
                };

                int remainingPeople = (int)request.NumberOfPeople;

                // Allocate tables to accommodate the required number of people
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
                    return BadRequest(new { title = "Not enough tables to accommodate the requested number of people." });
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
        [HttpDelete("Abort/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Fetch the booking and related tables
                var booking = await _context.Bookings
                    .Include(b => b.Tables) // Include related tables
                    .Include(b => b.Customer) // Include related customer
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (booking == null)
                {
                    return NotFound(); // Return 404 if the booking is not found
                }

                // Remove the booking reference from each related table
                foreach (var table in booking.Tables.ToList())
                {
                    table.Bookings.Remove(booking);
                    _context.Tables.Update(table);
                }

                // Remove the booking
                _context.Bookings.Remove(booking);

                // Remove the customer (if needed)
                if (booking.Customer != null)
                {
                    _context.Customers.Remove(booking.Customer);
                }

                // Save changes
                await _context.SaveChangesAsync();

                return NoContent(); // Return 204 No Content on successful deletion
            }
            catch (Exception ex)
            {
                // Log the exception (optional)
                Console.Error.WriteLine(ex);

                // Return a generic error message
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                // Fetch all bookings with related tables
                var bookings = await _context.Bookings
                    .Include(b => b.Tables) // Ensure that you include related tables
                    .Include(c => c.Customer)
                    .ToListAsync();

                // Map the bookings to a new anonymous type including the concatenated table names
                var detailBookings = bookings.Select(booking => new
                {
                    booking.Id,
                    booking.Customer.Name,
                    booking.Customer.Phone,
                    booking.Date,
                    booking.Time,
                    booking.NumberOfPeople,
                    booking.Note,
                    booking.DateBooking,
                    booking.DateStart,
                    booking.DateCheckOut,
                    BookingTable = string.Join(", ", booking.Tables.Select(t => t.Name)), // Concatenate table names
                                                                                          // booking.IsCheck
                }).ToList();

                return Ok(detailBookings);
            }
            catch (Exception ex)
            {
                // Log the exception (optional)
                Console.Error.WriteLine(ex);

                // Return a generic error message
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpGet("getDetailById/{id}")]
        public async Task<IActionResult> getDetailById(int id)
        {
            try
            {
                var booking = await _context.Bookings
                    .Include(b => b.Tables) // Assuming a relationship exists between Bookings and Tables
                    .Include(c => c.Customer)
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (booking == null)
                {
                    return NotFound();
                }

                // Concatenate table names
                var tableNames = booking.Tables.Select(t => t.Name).ToList();
                var tableNamesString = string.Join(", ", tableNames);

                var detailBooking = new
                {
                    booking.Id,
                    booking.Customer.Name,
                    booking.Customer.Phone,
                    booking.Date,
                    booking.Time,
                    booking.NumberOfPeople,
                    booking.Note,
                    booking.DateBooking,
                    booking.DateStart,
                    booking.DateCheckOut,
                    BookingTable = tableNamesString, // Concatenated table names
                    booking.IsCheck
                };

                return Ok(detailBooking);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("UpdateBooking")]
        public async Task<IActionResult> UpdateBooking([FromBody] BookingUpdateDTO req)
        {
            try
            {
                string jsonString = Request.Cookies["authToken"];
                var jsonObject = JsonConvert.DeserializeObject<dynamic>(jsonString);
                string jwt = jsonObject?.token;
                Console.WriteLine(jwt);
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(jwt) as JwtSecurityToken;

                if (jsonToken == null)
                {
                    return BadRequest(new { title = "JWT không hợp lệ." });
                }

                // Trích xuất thông tin từ payload
                var emailClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "email");

                if (emailClaim == null)
                {
                    return BadRequest(new { title = "Không tìm thấy email người dùng." });
                }

                var userEmail = emailClaim.Value.ToLower();

                var user = _context.Users.Where(x => x.Email.ToLower() == userEmail).FirstOrDefault();
                if (user == null)
                {
                    return BadRequest(new { title = "Người dùng không tồn tại." });
                }
                var detailBooking = await _context.Bookings
                    .Include(b => b.Tables) // Include related Tables for updating
                    .FirstOrDefaultAsync(x => x.Id == req.Id);

                // Cập nhật thông tin booking
                detailBooking.Note = req.Note;
                detailBooking.DateStart = req.DateStart;
                detailBooking.DateCheckOut = req.DateCheckOut;
                detailBooking.UserCheck = user.Id;

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
                var messageBody = System.Text.Json.JsonSerializer.Serialize(bookingMessage);
                var body = Encoding.UTF8.GetBytes(messageBody);

                channel.BasicPublish(exchange: "booking_exchange",
                                     routingKey: "booking",
                                     basicProperties: null,
                                     body: body);
            }
        }
    }
}
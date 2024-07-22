using BusinessObject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Project_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TableController : ControllerBase
    {
        private readonly PizzaLabContext _context;

        public TableController(PizzaLabContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult getAll()
        {
            try
            {
                var listTable = _context.Tables.ToList();
                return Ok(listTable);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("availableTables")]
        public async Task<IActionResult> GetAvailableTables()
        {
            try
            {

                DateTime dateStart = DateTime.Now;
                TimeSpan bookingDuration = TimeSpan.FromHours(2);
                DateTime dateCheckOut = dateStart + bookingDuration;

                // Fetch all tables
                var tables = await _context.Tables.ToListAsync();

                // Fetch all bookings for the requested date
                var bookings = await _context.Bookings
                    .Include(b => b.Tables) // Include related tables for each booking
                    .Where(b => b.Date.Value.Date == dateStart.Date)
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
                                     b.Date.Value.Date + b.Time < dateCheckOut &&
                                     b.Date.Value.Date + b.Time + bookingDuration > dateStart)
                                )
                            )
                        )
                    )
                    .ToList();
                // Map available tables to DTOs
                var availableTableDTOs = availableTables.Select(t => new 
                {
                    Id = t.Id,
                    Name = t.Name,
                    NumberOfPeople = t.NumberOfPeople
                }).ToList();

                return Ok(availableTableDTOs);
            }
            catch (Exception ex)
            {
                return BadRequest(new { title = $"Internal server error: {ex.Message}" });
            }
        }


    }
}

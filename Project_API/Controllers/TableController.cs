using BusinessObject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
    }
}

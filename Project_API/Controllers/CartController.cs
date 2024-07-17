using BusinessObject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Project_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly PizzaLabContext _context;

        public CartController(PizzaLabContext context)
        {
            _context = context;
        }
        [HttpGet] 
        public IActionResult GetCart() {
            try
            {
                return Ok();
                //var cart = _context.Carts.Include(X=> X.CartDetails).ToList()
                //    .Select();
            }catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

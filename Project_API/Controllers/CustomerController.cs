using BusinessObject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project_API.DTO;

namespace Project_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly PizzaLabContext _context;

        public CustomerController(PizzaLabContext context)
        {
            _context = context;
        }
        [HttpPost]
        public IActionResult AddCustomer(CustomerDTO customer)
        {
            try
            {
                Customer customer2 = new Customer()
                {
                    Phone = customer.Phone,
                    Email = customer.Email,
                    Name = customer.Name,
                };
                _context.Customers.Add(customer2);
                _context.SaveChanges();
                return Ok(customer2);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

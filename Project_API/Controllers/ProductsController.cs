using BusinessObject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Project_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        PizzaLabContext context = new PizzaLabContext();
        [HttpGet]
        public IActionResult getAllProduct()
        {
            var product = context.Products.ToList();
            return Ok(product);

        }
        [HttpGet("{id}")]
        public IActionResult getProductById(int id)
        {
            if (id > 0)
            {
                var product = from Product in context.Products
                              where Product.Id == id
                              select Product;
                return Ok(product);

            }
            else
            {
                return BadRequest();
            }
        }


        [HttpGet("[action]")]
        public IActionResult getProduct(int id)
        {
            if (id > 0)
            {
                var productWithCategory = (from p in context.Products
                                           join c in context.Categories on p.CategoryId equals c.Id
                                           where p.Id == id
                                           select new
                                           {
                                               p.Id,
                                               p.Name,
                                               p.Description,
                                               p.Image,
                                               p.Weight,
											   p.CategoryId,
                                               p.Ammount,
                                               p.Price,
                                               c.CategoryName
  
                                           }).FirstOrDefault();

                if (productWithCategory == null)
                {
                    return NotFound("Product not found");
                }

                return Ok(productWithCategory);

            }
            else
            {
                return BadRequest();
            }
        }
    }
}

using BusinessObject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_API.DTO;
using static System.Net.Mime.MediaTypeNames;

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
        [HttpGet("search")]
        public ActionResult<IEnumerable<Product>> SearchByName(string name)
        {
            var productsWithCategory = (from p in context.Products
                                        join c in context.Categories on p.CategoryId equals c.Id
                                        where EF.Functions.Like(p.Name, $"%{name}%")
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
                                            CategoryName = c.CategoryName
                                        }).ToList();

            if (!productsWithCategory.Any())
            {
                return NotFound("Product not found");
            }

            return Ok(productsWithCategory);
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProducts()
        {
            var products = await context.Products
                .Include(p => p.Category) // Bao gồm thông tin danh mục
                .ToListAsync();

            var productDtos = products.Select(product => new ProductDTO
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Weight = product.Weight,
                Image = product.Image, // Giả sử bạn lưu hình ảnh dưới dạng Base64
                Ammount = product.Ammount,
                CategoryName = product.Category.CategoryName // Lấy tên danh mục
            }).ToList();

            return Ok(productDtos);
        }

        [HttpGet("SearchProductByName/{name}")]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> SearchProductByName(string name)
        {
            var lowerCaseName = name.ToLower();

            var productDtos = await context.Products
                .Where(p => p.Name.ToLower().Contains(lowerCaseName))
                .Select(p => new ProductDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Weight = p.Weight,
                    Image = p.Image, // Giả sử bạn lưu hình ảnh dưới dạng Base64
                    Ammount = p.Ammount,
                    CategoryName = p.Category.CategoryName // Lấy tên danh mục
                })
                .ToListAsync();

            return Ok(productDtos);
        }

        [HttpPost("AddProduct")]
        public async Task<ActionResult<Product>> PostProduct([FromForm] ProductDTO productDto)
        {
            if (productDto.ImageFile != null && productDto.ImageFile.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    await productDto.ImageFile.CopyToAsync(ms);
                    var fileBytes = ms.ToArray();
                    productDto.Image = Convert.ToBase64String(fileBytes);
                }
            }

            var product = new Product
            {
                Name = productDto.Name,
                Description = productDto.Description,
                Image = productDto.Image,
                Weight = productDto.Weight,
                CategoryId = productDto.CategoryId,
                Ammount = productDto.Ammount,
                Price = productDto.Price,
            };

            context.Products.Add(product);
            await context.SaveChangesAsync();

            return CreatedAtAction("GetProduct", new { id = product.Id }, product);
        }
        [HttpPut("UpdateProduct/{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] ProductDTO productDto)
        {
            if (id != productDto.Id)
            {
                return BadRequest("Product ID mismatch");
            }

            var product = await context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound($"Product with ID {id} not found.");
            }

            product.Name = productDto.Name;
            product.Description = productDto.Description;
            if (productDto.ImageFile != null && productDto.ImageFile.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    await productDto.ImageFile.CopyToAsync(ms);
                    var fileBytes = ms.ToArray();
                    product.Image = Convert.ToBase64String(fileBytes);
                }
            }
            product.Weight = productDto.Weight;
            product.CategoryId = productDto.CategoryId;
            product.Ammount = productDto.Ammount;
            product.Price = productDto.Price;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(product);
        }

        private bool ProductExists(int id)
        {
            return context.Products.Any(e => e.Id == id);
        }
        [HttpDelete("DeleteProduct/{id}")]
        //[Authorize]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await context.Products
                .Include(p => p.OrderDetails)
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound($"Product with ID {id} not found.");
            }

           
            context.OrderDetails.RemoveRange(product.OrderDetails);
            context.Reviews.RemoveRange(product.Reviews);

            
            context.Products.Remove(product);
            await context.SaveChangesAsync();

            return NoContent(); 
        }
    }
}

using BusinessObject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Project_API.DTO;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace Project_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        PizzaLabContext context = new PizzaLabContext();
        [HttpPost]
        public IActionResult createOrder([FromBody] OrderDTO or)
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

                var user = context.Users.FirstOrDefaultAsync(x => x.Email.ToLower() == userEmail);
                if (user == null)
                {
                    return BadRequest(new { title = "Người dùng không tồn tại." });
                }
                Customer customer = new Customer()
                {
                    Name = or.Customer.Name ?? "Unknown Name", // Nếu Name là null, sử dụng "Unknown Name"
                    Email = or.Customer.Email ?? "unknown@example.com", // Nếu Email là null, sử dụng "unknown@example.com"
                    Phone = or.Customer.Phone ?? "000-000-0000" // Nếu Phone là null, sử dụng "000-000-0000"
                };

                context.Customers.Add(customer);
                Order order = new Order()
                {
                    OrderDate = DateTime.Now,
                    UserId = user.Id,
                    Status = or.Status,
                    Freight = or.Freight,
                    TableAdress = or.TableAdress,
                    Customer = customer,
                    IsCheck = false,
                };
                context.Orders.Add(order);
                context.SaveChanges();

                foreach (var detail in or.OrderDetails)
                {
                    var orderDetail = new OrderDetail
                    {
                        OrderId = order.Id,
                        ProductId = detail.ProductId,
                        Quantity = detail.Quantity,
                        Price = detail.Price
                    };

                    context.OrderDetails.Add(orderDetail);
                }
                context.SaveChanges();
                return Ok();

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            try
            {
                var orders = await context.Orders
                    .Include(o => o.Customer)  // Nếu bạn muốn bao gồm thông tin khách hàng
                    .Select(o => new
                    {
                        o.Id,
                        o.OrderDate,
                        o.Status,
                        o.Freight,
                        o.TableAdress,
                        CustomerName = o.Customer.Name,
                        CustomerEmail = o.Customer.Email
                    })
                    .ToListAsync();

                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(new { title = "Lỗi khi lấy danh sách đơn hàng.", message = ex.Message });
            }
        }
        [HttpGet("GetOrderDetails/{orderId}")]
        public async Task<IActionResult> GetOrderDetails(int orderId)
        {
            try
            {
                var order = await context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product) // Nếu bạn muốn bao gồm thông tin sản phẩm
                    .Where(o => o.Id == orderId)
                    .Select(o => new
                    {
                        o.Id,
                        o.OrderDate,
                        o.Status,
                        o.Freight,
                        o.TableAdress,
                        Customer = new
                        {
                            o.Customer.Name,
                            o.Customer.Email,
                            o.Customer.Phone
                        },
                        OrderDetails = o.OrderDetails.Select(od => new
                        {
                            od.ProductId,
                            od.Quantity,
                            od.Price,
                            ProductName = od.Product.Name // Nếu bạn bao gồm thông tin sản phẩm
                        })
                    })
                    .FirstOrDefaultAsync();

                if (order == null)
                {
                    return NotFound(new { title = "Đơn hàng không tồn tại." });
                }

                return Ok(order);
            }
            catch (Exception ex)
            {
                return BadRequest(new { title = "Lỗi khi lấy chi tiết đơn hàng.", message = ex.Message });
            }
        }

    }
}

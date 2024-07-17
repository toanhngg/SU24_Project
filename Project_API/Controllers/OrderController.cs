using BusinessObject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_API.DTO;
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
                    UserId = 1,
                    Status = or.Status,
                  //  RequiredDate = or.RequiredDate, // Ngày tạo
                  //  ShippedDate = or.ShippedDate,   // Hoàn thành
                    Freight = or.Freight,
                    TableAdress = or.TableAdress,
                    Customer = customer,
                    //IsCart = or.IsCart,
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
    }
}

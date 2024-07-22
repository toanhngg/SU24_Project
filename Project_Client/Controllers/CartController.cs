using BusinessObject.Models;
using Microsoft.AspNetCore.Mvc;
using Project_Client.Models;
using Project_Client.Util;
using System.Collections.Generic;
using System.Linq;

namespace Project_Client.Controllers
{
    public class CartController : Controller
    {
        public IActionResult Index()
        {
            // Retrieve cart from session
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            return View(cart);
        }

        // Add product to cart
        [HttpPost]
        public IActionResult AddToCart(int productId, string productName, string productImage, decimal price, int quantity)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

            var existingItem = cart.SelectMany(c => c.OrderDetails).FirstOrDefault(od => od.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                // If the cart is empty or the product is not found, create a new entry
                var cartItem = cart.FirstOrDefault(c => c.Id == productId);
                if (cartItem == null)
                {
                    cartItem = new CartItem
                    {
                        Id = productId, // or another identifier if needed
                        OrderDetails = new List<CartItem.OrderDetail>()
                    };
                    cart.Add(cartItem);
                }

                cartItem.OrderDetails.Add(new CartItem.OrderDetail
                {
                    ProductId = productId,
                    ProductName = productName,
                    Price = price,
                    Quantity = quantity
                });
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return Json(new { success = true, message = "Product added to cart successfully!" });
        }

        // Remove product from cart
        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart");
            if (cart != null)
            {
                foreach (var item in cart)
                {
                    var detail = item.OrderDetails.FirstOrDefault(od => od.ProductId == productId);
                    if (detail != null)
                    {
                        item.OrderDetails.Remove(detail);
                        if (!item.OrderDetails.Any())
                        {
                            cart.Remove(item);
                        }
                        HttpContext.Session.SetObjectAsJson("Cart", cart);
                        return Json(new { success = true });
                    }
                }
            }

            return Json(new { success = false });
        }

        // Update cart item
        [HttpPost]
        public IActionResult UpdateCartItem(int productId, int quantity)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

            foreach (var item in cart)
            {
                var cartItem = item.OrderDetails.FirstOrDefault(od => od.ProductId == productId);
                if (cartItem != null)
                {
                    cartItem.Quantity = quantity;
                    HttpContext.Session.SetObjectAsJson("Cart", cart);
                    return Json(new { success = true });
                }
            }

            return Json(new { success = false });
        }

        // Submit order
        [HttpPost]
        public IActionResult SubmitOrder()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

            if (cart != null && cart.Any())
            {
                var orderDetails = cart.SelectMany(item => item.OrderDetails).Select(od => new CartItem.OrderDetail
                {
                    ProductId = od.ProductId,
                    ProductName = od.ProductName,
                    Price = od.Price,
                    Quantity = od.Quantity,
                    Check = true // Set Check to true for the first submission
                }).ToList();
                Console.Write(orderDetails);
                // Handle the order submission (e.g., save to database, send to API, etc.)
                // Example:
                // await _orderService.PlaceOrder(new Order { OrderDetails = orderDetails });

                // Clear the cart after submission
                HttpContext.Session.SetObjectAsJson("Cart", new List<CartItem>());

                return Json(new { success = true, message = "Order submitted successfully!" });
            }

            return Json(new { success = false, message = "Cart is empty!" });
        }
    }
}

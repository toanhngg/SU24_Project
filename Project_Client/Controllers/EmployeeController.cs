using BusinessObject.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project_Client.Models;
using Project_Client.Util;
using System.Net.Http.Headers;
using System.Text;

namespace Project_Client.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly HttpClient client = null;
        private string UserApiUrl = "";
        private string ProductApiUrl = "";
        private string CategoryApiUrl = "";

        public EmployeeController()
        {
            client = new HttpClient();
            var content = new MediaTypeWithQualityHeaderValue("application/json");
            client.DefaultRequestHeaders.Accept.Add(content);
            UserApiUrl = "https://localhost:7135/api/Login";
            ProductApiUrl = "https://localhost:7135/api/Products";
            CategoryApiUrl = "https://localhost:7135/api/Category";

        }
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login1(string email, string password)
        {
            using (HttpClient client = new HttpClient())
            {
                // Tạo đối tượng chứa dữ liệu đăng nhập
                var loginData = new
                {
                    Email = email,
                    Password = password
                };

                // Chuyển đối tượng thành JSON
                var json = JsonConvert.SerializeObject(loginData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Gửi yêu cầu POST với body
                using (HttpResponseMessage res = await client.PostAsync(UserApiUrl, content))
                {
                    if (res.IsSuccessStatusCode)
                    {
                        // Xử lý phản hồi thành công
                        var responseContent = await res.Content.ReadAsStringAsync();
                        // Có thể lưu JWT hoặc thông tin người dùng ở đây
                        HttpContext.Session.SetString("UserEmail", email); // Lưu thông tin vào session
                                                                           // Gửi JWT về client dưới dạng cookie
                        Response.Cookies.Append("authToken", responseContent, new CookieOptions
                        {
                            HttpOnly = true, // Cookie chỉ có thể được truy cập từ server, không thể truy cập từ JavaScript
                            Secure = true, // Chỉ gửi cookie qua HTTPS
                            SameSite = SameSiteMode.None, // Chỉ gửi cookie trong cùng một trang
                            Expires = DateTime.UtcNow.AddHours(1), // Thời gian hết hạn cho cookie
                            Path = "/" // Đảm bảo rằng cookie có thể được gửi đến mọi đường dẫn
                        });

                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        // Xử lý lỗi
                        ModelState.AddModelError("", "Invalid user data");
                        return View();
                    }
                }
            }
        }
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("UserEmail"); // Xóa thông tin khỏi session
            return RedirectToAction("Login", "Admin");
        }

        public async Task<IActionResult> Index()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (userEmail == null)
            {
                return RedirectToAction("Login", "Home"); // Nếu không có session, chuyển hướng đến trang đăng nhập
            }

            using (HttpResponseMessage res = await client.GetAsync(ProductApiUrl))
            {
                using (HttpContent content = res.Content)
                {
                    string data = await content.ReadAsStringAsync();
                    List<Product> product = JsonConvert.DeserializeObject<List<Product>>(data);
                    ViewBag.Product = product;

                }

            }
            using (HttpResponseMessage res = await client.GetAsync(CategoryApiUrl))
            {
                using (HttpContent content = res.Content)
                {
                    string data = await content.ReadAsStringAsync();
                    List<Category> category = JsonConvert.DeserializeObject<List<Category>>(data);
                    ViewBag.Category = category;

                }

            }
            return View();
        }

        public IActionResult Dashboard()
        {
            return View();
        }
        public IActionResult IndexProduct()
        {
            return View();
        }
        public IActionResult IndexCategory()
        {
            return View();
        }

        public IActionResult IndexUser()
        {
            return View();
        }
        public IActionResult IndexOrder()
        {
            return View();
        }
    }
}

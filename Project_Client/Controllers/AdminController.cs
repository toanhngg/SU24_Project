using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project_API.Controllers;
using Project_API.DTO;
using System.Net.Http.Headers;
using System.Text;

namespace Project_Client.Controllers
{
    public class AdminController : Controller
    {
        private readonly HttpClient client = null;
        private string UserApiUrl = "";

        public IActionResult Privacy()
        {
            return View();
        }
        public AdminController()
        {
            client = new HttpClient();
            var content = new MediaTypeWithQualityHeaderValue("application/json");
            client.DefaultRequestHeaders.Accept.Add(content);
            UserApiUrl = "https://localhost:7135/api/Login";

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
                            SameSite = SameSiteMode.Strict, // Chỉ gửi cookie trong cùng một trang
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
        public IActionResult IndexOrder()
        {
            return View();
        }
    }
}

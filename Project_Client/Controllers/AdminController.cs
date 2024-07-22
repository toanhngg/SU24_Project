using BusinessObject.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project_API.Controllers;
using Project_API.DTO;
using Project_Client.Models;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Numerics;
using System.Security.Claims;
using System.Text;
using UserRegister = Project_API.DTO.UserRegister;

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
        public IActionResult Register()
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
                        var jsonObject = JsonConvert.DeserializeObject<dynamic>(responseContent);
                        string jwt = jsonObject?.token;
                        Console.WriteLine(jwt);                                                  // Gửi JWT về client dưới dạng cookie
                        Response.Cookies.Append("authToken", responseContent, new CookieOptions
                        {
                            HttpOnly = true, // Cookie chỉ có thể được truy cập từ server, không thể truy cập từ JavaScript
                            Secure = true, // Chỉ gửi cookie qua HTTPS
                            SameSite = SameSiteMode.Strict, // Chỉ gửi cookie trong cùng một trang
                            Expires = DateTime.UtcNow.AddHours(1), // Thời gian hết hạn cho cookie
                            Path = "/" // Đảm bảo rằng cookie có thể được gửi đến mọi đường dẫn
                        });
                        string roleId = ParseJwt(jwt);
                        HttpContext.Session.SetString("UserEmail", email); // Lưu thông tin vào session
                        HttpContext.Session.SetString("UserRole", roleId); // Lưu thông tin vào session

                        TempData["RoleId"] = roleId;// Store the RoleId in TempData
                        if (roleId == "1")
                        {
                            return RedirectToAction("Index", "Employee");

                        }
                        else if (roleId == "3")
                        {
                            return RedirectToAction("Index", "Home");

                        }
                        else if (roleId == "2")
                        {
                            return RedirectToAction("Dashboard", "Admin");
                        }
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
        public string ParseJwt(string token)
        {
            try
            {
                // Tạo một JwtSecurityTokenHandler
                var handler = new JwtSecurityTokenHandler();

                // Đọc token và phân tích nó thành JwtSecurityToken
                var jwtToken = handler.ReadJwtToken(token);

                // Truy xuất các claim từ payload của token
                //var claims = jwtToken.Claims.ToDictionary(c => c.Type, c => c.Value);
                //  var claims = jwtToken.Claims.FirstOrDefault(c => c.Type == "role");
                var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);

                if (roleClaim != null)
                {
                    return roleClaim.Value.ToString();
                }
                else
                {
                    return ("Role claim not found.");
                }


            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpPost]
        public async Task<IActionResult> Register1([FromForm] UserRegister user)
        {
            using (HttpClient client = new HttpClient())
            {
                // Tạo đối tượng chứa dữ liệu đăng nhập
                var registerData = new
                {
                    Email = user.Email,
                    Password = user.Password,
                    Phone = user.Phone,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    ContactName = user.ContactName,
                    RoleId = 3,
                    Active = true
                };

                // Chuyển đối tượng thành JSON
                var json = JsonConvert.SerializeObject(registerData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Gửi yêu cầu POST với body
                using (HttpResponseMessage res = await client.PostAsync("https://localhost:7135/api/Login/register", content))
                {
                    if (res.IsSuccessStatusCode)
                    {
                        // Xử lý phản hồi thành công
                        var responseContent = await res.Content.ReadAsStringAsync();
                        // Có thể lưu JWT hoặc thông tin người dùng ở đây
                        HttpContext.Session.SetString("UserEmail", user.Email); // Lưu thông tin vào session
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

        public IActionResult IndexUser()
        {
            return View();
        }
        public IActionResult IndexOrder()
        {
            return View();
        }
        public IActionResult IndexChart()
        {
            return View();
        }
        public IActionResult ViewHomePage()
        {
            return RedirectToAction("Index", "Home");
        }
    }
}

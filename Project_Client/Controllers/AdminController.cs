using BusinessObject.Models;
using GSF.IO;
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
        [HttpPost("upload")]
        public async Task<IActionResult> Test(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Không có tệp tin nào được chọn.");
            }

            string url = "https://34.150.7.40:3001/upload"; // URL của API bên ngoài

            try
            {
                using (HttpClient client = new HttpClient())
                using (MultipartFormDataContent form = new MultipartFormDataContent())
                {
                    // Chuyển đổi IFormFile thành ByteArrayContent
                    using (var memoryStream = new MemoryStream())
                    {
                        await file.CopyToAsync(memoryStream);
                        ByteArrayContent fileContent = new ByteArrayContent(memoryStream.ToArray());
                        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

                        // Thay đổi "file" bằng tên tham số mà API yêu cầu để nhận tệp
                        form.Add(fileContent, "file", file.FileName);

                        // Gửi yêu cầu POST đến API bên ngoài
                        HttpResponseMessage response = await client.PostAsync(url, form);

                        // Kiểm tra kết quả
                        if (response.IsSuccessStatusCode)
                        {
                            return Ok("Tệp tin đã được tải lên thành công.");
                        }
                        else
                        {
                            string errorMessage = await response.Content.ReadAsStringAsync();
                            return StatusCode((int)response.StatusCode, $"Lỗi: {response.StatusCode} - {errorMessage}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Đã xảy ra lỗi: {ex.Message}");
            }
        }
 

    public IActionResult Login()
        {
            return View();
        }
        public IActionResult Register()
        {
            return View();
        }
        //[HttpPost]
        //public async Task<IActionResult> Login1(string email, string password)
        //{
        //    using (HttpClient client = new HttpClient())
        //    {
        //        // Tạo đối tượng chứa dữ liệu đăng nhập
        //        var loginData = new
        //        {
        //            Email = email,
        //            Password = password
        //        };

        //        // Chuyển đối tượng thành JSON
        //        var json = JsonConvert.SerializeObject(loginData);
        //        var content = new StringContent(json, Encoding.UTF8, "application/json");

        //        // Gửi yêu cầu POST với body
        //        using (HttpResponseMessage res = await client.PostAsync(UserApiUrl, content))
        //        {
        //            if (res.IsSuccessStatusCode)
        //            {
        //                // Xử lý phản hồi thành công
        //                var responseContent = await res.Content.ReadAsStringAsync();
        //                // Có thể lưu JWT hoặc thông tin người dùng ở đây
        //                var jsonObject = JsonConvert.DeserializeObject<dynamic>(responseContent);
        //                string jwt = jsonObject?.token;
        //                Console.WriteLine(jwt);                                                  // Gửi JWT về client dưới dạng cookie
        //                                                                                         //Response.Cookies.Append("toanhToken", responseContent, new CookieOptions
        //                                                                                         //{
        //                                                                                         //    HttpOnly = true, // Cookie chỉ có thể được truy cập từ server, không thể truy cập từ JavaScript
        //                                                                                         //    Secure = true, // Chỉ gửi cookie qua HTTPS
        //                                                                                         //    SameSite = SameSiteMode.None, // Chỉ gửi cookie trong cùng một trang
        //                                                                                         //    Expires = DateTime.UtcNow.AddHours(1), // Thời gian hết hạn cho cookie
        //                                                                                         //    Path = "/" // Đảm bảo rằng cookie có thể được gửi đến mọi đường dẫn
        //                                                                                         //});

        //                var cookieOptions = new CookieOptions
        //                {
        //                    HttpOnly = true, // Bảo vệ cookie khỏi truy cập từ JavaScript
        //                    Secure = true,   // Cookie chỉ được gửi qua HTTPS
        //                    Expires = DateTime.Now.AddHours(1)
        //                };

        //                // Gửi cookie xác thực về cho client
        //                Response.Cookies.Append("authToken", jwt, cookieOptions);

        //                string roleId = ParseJwt(jwt);
        //                HttpContext.Session.SetString("UserEmail", email); // Lưu thông tin vào session
        //                HttpContext.Session.SetString("UserRole", roleId); // Lưu thông tin vào session

        //                TempData["RoleId"] = roleId;// Store the RoleId in TempData
        //                if (roleId == "1")
        //                {
        //                    return RedirectToAction("Index", "Employee");

        //                }
        //                else if (roleId == "3")
        //                {
        //                    return RedirectToAction("Index", "Home");

        //                }
        //                else if (roleId == "2")
        //                {
        //                    return RedirectToAction("Dashboard", "Admin");
        //                }
        //                return RedirectToAction("Index", "Home");

        //            }
        //            else
        //            {
        //                // Xử lý lỗi
        //                ModelState.AddModelError("", "Invalid user data");
        //                return View();
        //            }
        //        }
        //    }
        //}
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
                        Response.Cookies.Append("authToken", jwt, new CookieOptions
                        {
                            HttpOnly = true, // Cookie chỉ có thể được truy cập từ server, không thể truy cập từ JavaScript
                            Secure = true, // Chỉ gửi cookie qua HTTPS
                            Expires = DateTime.Now.AddHours(1), // Thời gian hết hạn cho cookie
                        });
                        string roleId = ParseJwt(jwt);
                        //HttpContext.Session.SetString("UserEmail", email); // Lưu thông tin vào session
                        //HttpContext.Session.SetString("UserRole", roleId); // Lưu thông tin vào session

                        //TempData["RoleId"] = roleId;// Store the RoleId in TempData
                       // var roleId = User.FindFirst(ClaimTypes.Role)?.Value;
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
                        Response.Cookies.Append("toanhToken", responseContent, new CookieOptions
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

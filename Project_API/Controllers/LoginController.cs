using BusinessObject.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Project_API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Project_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly PizzaLabContext _context;
        private readonly ILogger<LoginController> _logger;
        private readonly IConfiguration _config;

        public LoginController(PizzaLabContext context, ILogger<LoginController> logger, IConfiguration config)
        {
            _context = context;
            _logger = logger;
            _config = config;
        }
        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login([FromBody] UserModel login)
        {
            // Xác thực người dùng
            var user = AuthenticateUser(login);
            if (user == null)
            {
                return Unauthorized(); // Trả về Unauthorized nếu người dùng không hợp lệ
            }

            // Tạo JWT
            var tokenString = GenerateJSONWebToken(user);

       

            // Trả về JWT trong response body nếu cần
            return Ok(new { token = tokenString });
        }

        [HttpGet("GetAuthToken")]
public IActionResult GetAuthToken()
{
    var authToken = Request.Cookies["authToken"];
    if (string.IsNullOrEmpty(authToken))
    {
        return BadRequest(new { title = "Cookie không tồn tại hoặc không có giá trị." });
    }
    return Ok(new { authToken = authToken });
}


        private string GenerateJSONWebToken(UserModel userInfo)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Email, userInfo.Email),
                new Claim(ClaimTypes.Role, userInfo.RoleId.ToString()) };

            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
              _config["Jwt:Issuer"],
              claims,
              expires: DateTime.Now.AddMinutes(120),
              signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);

        }

        private UserModel AuthenticateUser(UserModel login)
        {
            UserModel user = null;
            User us = _context.Users.Where(x => x.Email == login.Email && x.Password == login.Password).FirstOrDefault();
            //Validate the User Credentials
            //Demo Purpose, I have Passed HardCoded User Information
            if (us != null)
            {
                user = new UserModel
                {
                    Email = us.Email,
                    RoleId = us.RoleId
                };
            }
            return user;
        }
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            // Xóa cookie xác thực
            await HttpContext.SignOutAsync();

            // Trả về phản hồi thành công
            return Ok(new { message = "Logged out successfully" });
        }
        [HttpGet]
        [Authorize]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2", "value3", "value4", "value5" };
        }
    }
}

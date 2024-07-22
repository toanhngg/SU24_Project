using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using System.Text;

namespace Project_API.MiddeWare
{
    public class JwtCookieMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly string _jwtSecret;

        public JwtCookieMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
            _jwtSecret = _configuration["Jwt:Key"] = null!;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path;//path cua request nguoi dung dang huong toi nhung chua co quyen truy cap
            var token = context.Request.Cookies["authToken"];
            
            if (!string.IsNullOrEmpty(token))
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_jwtSecret);

                try
                {
                    tokenHandler.ValidateToken(token, new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ClockSkew = TimeSpan.Zero
                    }, out SecurityToken validatedToken);

                    if (validatedToken != null)
                    {
                        context.User = new ClaimsPrincipal(new ClaimsIdentity(tokenHandler.ReadJwtToken(token).Claims));
                    }
                }
                catch (Exception ex)
                {
                    await context.ChallengeAsync("JwtCookieMiddleware", new AuthenticationProperties { RedirectUri = $"/User/Login?targetURL={path}" });


                }
            }
            await context.ChallengeAsync("JwtCookieMiddleware", new AuthenticationProperties { RedirectUri = $"/User/Login?targetURL={path}" });
        }
    }
}

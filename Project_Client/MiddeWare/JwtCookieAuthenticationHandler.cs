using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;

namespace Pizza_Client.MiddeWare
{
    public class JwtCookieAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IConfiguration _configuration;

        public JwtCookieAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, IConfiguration configuration) : base(options, logger, encoder, clock)
        {
            _configuration = configuration;
        }
        private string RedirectUri;
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var path = Request.Path;//path cua request nguoi dung dang huong toi nhung chua co quyen truy cap
            // Xử lý logic xác thực cho scheme "JwtCookieMiddleware"
            // Trả về AuthenticateResult.Success hoặc AuthenticateResult.Fail tùy vào kết quả xác thực

            // Ví dụ:
            var token = Request.Cookies["authToken"];
            var endpoint = Context.GetEndpoint();

            if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null || Request.Path.StartsWithSegments("/lib")|| Request.Path.StartsWithSegments("/css")|| Request.Path.StartsWithSegments("/js"))
            {
                return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(), Scheme.Name)));
            }
            if (!string.IsNullOrEmpty(token))
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

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
                        // Thiết lập principal cho request
                        Context.User = new ClaimsPrincipal(new ClaimsIdentity(tokenHandler.ReadJwtToken(token).Claims));

                        // Trả về AuthenticateResult.Success nếu xác thực thành công
                        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(Context.User, Scheme.Name)));
                    }
                }
                catch
                {
                    return Task.FromResult(AuthenticateResult.Fail("Authentication failed."));
                }
            }
            return Task.FromResult(AuthenticateResult.NoResult());

        }

        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            //var redirectUri = properties?.RedirectUri ?? "/login";
            Response.Redirect(RedirectUri);
            await Task.CompletedTask;
        }
    }

}

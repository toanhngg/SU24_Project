using BusinessObject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Project_API.DTO;
using System.IdentityModel.Tokens.Jwt;

namespace Project_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        PizzaLabContext _context = new PizzaLabContext();

        [HttpGet("{id}")]
        public async Task<IActionResult> GetReviews(int id)
        {
            string currentUserEmail = null;

            // Try to extract the current user email from the JWT token
            try
            {
                string jsonString = Request.Cookies["authToken"];
                if (!string.IsNullOrEmpty(jsonString))
                {
                    var jsonObject = JsonConvert.DeserializeObject<dynamic>(jsonString);
                    string jwt = jsonObject?.token;
                    var handler = new JwtSecurityTokenHandler();
                    var jsonToken = handler.ReadToken(jwt) as JwtSecurityToken;

                    if (jsonToken != null)
                    {
                        var emailClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "email");
                        currentUserEmail = emailClaim?.Value.ToLower();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                Console.WriteLine("Error parsing JWT token: " + ex.Message);
            }

            var reviews = await _context.Reviews.Include(x => x.User)
                .Where(x => x.ProductId == id)
                .Select(x => new
                {
                    UserId = x.UserId,
                    UserName = x.User.FirstName + " " + x.User.LastName,
                    Comment = x.Comment,
                    DateModified = x.DateModified,
                    ProductId = id,
                    CanDelete = currentUserEmail != null && x.User.Email.ToLower() == currentUserEmail // Check if current user is the author
                })
                .ToListAsync();

            return Ok(reviews);
        }



        [HttpPost]
        public async Task<IActionResult> AddReview([FromBody] ReviewDTO review)
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

            var user = _context.Users.Where(x => x.Email.ToLower() == userEmail).FirstOrDefault();
            if (user == null)
            {
                return BadRequest(new { title = "Người dùng không tồn tại." });
            }
            if (review == null)
            {
                return BadRequest();
            }
            Review review1 = new Review()
            {
                Comment = review.Comment,
                UserId = user.Id,
                ProductId = review.ProductId,
                DateModified = DateTime.Now

            };
            review.DateModified = DateTime.Now;
            _context.Reviews.Add(review1);
            await _context.SaveChangesAsync();

            return Ok(review1);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound(new { success = false, message = "Review not found" });
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Review deleted successfully" });
        }
    }
}

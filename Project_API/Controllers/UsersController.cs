using BusinessObject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata.Ecma335;

namespace Project_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
       private readonly PizzaLabContext context ;


        public UsersController(PizzaLabContext _context )
        {
           context = _context;
        } 
		[HttpGet]
        public IActionResult getAllUser()
        {
            try
            {
				var allUser = context.Users.ToList();
				return Ok(allUser);
			}catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }


        }
		[HttpPost("[action]")]
		public async Task<IActionResult> getAuthentication(string email, string password)
        {
            User user = null;
            try
            {


            }catch (Exception ex)
            {

            }
            return Ok();

        }
		/*
           [HttpPost("[action]")]
        public IActionResult GetAnUserLogin(string email,string password)
        {
            User user = null;
            try
            {
                List<User> users = userRepository.GetUsers().ToList();
                user = users.FirstOrDefault(u => u.Email == email&& u.Password == password);
            }
            catch (Exception ex)
            {
                return Conflict("No User In DB");
            }
            if (user == null)
                return NotFound("User doesnt exist");
            return Ok(mapper.Map<UserDTO>(user));
        }
        
        */

	}
}

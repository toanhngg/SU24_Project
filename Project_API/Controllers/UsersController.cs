using BusinessObject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project_API.DTO;
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
        [HttpGet("GetUserDetail/{id}")]
        public async Task<ActionResult<UserDTO>> GetUserDetail(int id)
        {
            var user = await context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            var userDto = new UserDTO
            {
                Id = user.Id,
                Email = user.Email,
                Phone = user.Phone,
                FirstName = user.FirstName,
                LastName = user.LastName,
                ContactName = user.ContactName,
                RoleId = user.RoleId,
                Active = user.Active
            };

            return Ok(userDto);
        }
        [HttpPut("UpdateUser/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateDTO userUpdateDto)
        {
            var user = await context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            user.Phone = userUpdateDto.Phone ?? user.Phone;
            user.FirstName = userUpdateDto.FirstName ?? user.FirstName;
            user.LastName = userUpdateDto.LastName ?? user.LastName;
            user.ContactName = userUpdateDto.ContactName ?? user.ContactName;

            await context.SaveChangesAsync();

            return Ok(user);
        }
        [HttpPost("ChangeStatusUser/{id}")]
        public async Task<IActionResult> ChangeStatusUser(int id)
        {
            var user = await context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            
            user.Active = !user.Active;

            await context.SaveChangesAsync();

            return Ok(new { Active = user.Active });
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

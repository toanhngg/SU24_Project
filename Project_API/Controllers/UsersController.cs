using BusinessObject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        [HttpGet("api/Roles/{id}")]
        public IActionResult GetRoleById(int id)
        {
            var role = context.Roles.Find(id);
            if (role == null)
            {
                return NotFound();
            }
            return Ok(role);
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

        [HttpGet("search/{name}")]
        public async Task<ActionResult<IEnumerable<User>>> SearchUsers(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return BadRequest("Search name cannot be empty.");
            }

            var users = await context.Users
                .Where(u => u.FirstName.Contains(name) || u.LastName.Contains(name))
                .ToListAsync();

            return Ok(users);
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

        [HttpPost("CreateUser")]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateDTO userCreateDto)
        {
            if (userCreateDto == null)
            {
                return BadRequest("User data is required.");
            }

            if (string.IsNullOrEmpty(userCreateDto.Email))
            {
                return BadRequest("Email is required.");
            }

            // Tạo đối tượng User mới với mật khẩu mặc định
            var user = new User
            {
                Email = userCreateDto.Email,
                Phone = userCreateDto.Phone,
                FirstName = userCreateDto.FirstName,
                LastName = userCreateDto.LastName,
                ContactName = userCreateDto.ContactName,
                RoleId = userCreateDto.RoleId,
                Active = true,
                Password = "123456" // Mật khẩu mặc định
            };

            // Thêm người dùng vào cơ sở dữ liệu
            context.Users.Add(user);
            await context.SaveChangesAsync();

            return Ok(new { Message = "User created successfully.", UserId = user.Id });
        }

        [HttpGet("roles")]
        public async Task<IActionResult> GetAllRoles()
        {
            try
            {
                var roles = await context.Roles.ToListAsync();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
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

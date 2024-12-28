using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.Extensions.Logging;
using ERP_API.Data;
using ERP_API.Moduls; // Assuming User, UserCreateDto, UserReadOnlyDto exist
using ERP_API.Static; // Assuming Messages.Error500Message exists

namespace ERP_API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly Lg202324Context _context;
        private readonly ILogger<UsersController> _logger;
        private readonly IMapper _mapper;

        public UsersController(Lg202324Context context, ILogger<UsersController> logger, IMapper mapper)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserReadOnlyDto>>> GetUsers()
        {
            _logger.LogInformation($"Request to {nameof(GetUsers)}");
            try
            {
                var users = await _context.User.ToListAsync();
                var usersDto = _mapper.Map<IEnumerable<UserReadOnlyDto>>(users);
                return Ok(usersDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception in {nameof(GetUsers)}");
                return StatusCode(500, Messages.Error500Message);
            }
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserReadOnlyDto>> GetUser(int id)
        {
            _logger.LogInformation($"Request to {nameof(GetUser)} for ID: {id}");
            try
            {
                var user = await _context.User.FindAsync(id);
                if (user == null)
                {
                    _logger.LogWarning($"User with ID {id} not found in {nameof(GetUser)}");
                    return NotFound();
                }

                var userDto = _mapper.Map<UserReadOnlyDto>(user);
                return Ok(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception in {nameof(GetUser)}");
                return StatusCode(500, Messages.Error500Message);
            }
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, UserCreateDto userDto)
        {
            if (id != userDto.UserId)
            {
                _logger.LogWarning($"Update ID mismatch in {nameof(PutUser)} - ID: {id}");
                return BadRequest();
            }

            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                _logger.LogWarning($"User with ID {id} not found in {nameof(PutUser)}");
                return NotFound();
            }

            // Map DTO to entity
            _mapper.Map(userDto, user);
            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!UserExists(id))
                {
                    _logger.LogWarning($"User with ID {id} not found during PUT in {nameof(PutUser)}");
                    return NotFound();
                }
                else
                {
                    _logger.LogError(ex, $"Concurrency exception PUT in {nameof(PutUser)}");
                    return StatusCode(500, Messages.Error500Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception in {nameof(PutUser)}");
                return StatusCode(500, Messages.Error500Message);
            }

            return NoContent();
        }

        // POST: api/Users
        [HttpPost]
        public async Task<ActionResult<UserReadOnlyDto>> PostUser(UserCreateDto userDto)
        {
            _logger.LogInformation($"Request to {nameof(PostUser)}");
            try
            {
                // Map DTO to entity
                var user = _mapper.Map<User>(userDto);
                _context.User.Add(user);
                await _context.SaveChangesAsync();

                // Map entity to DTO
                var userReadOnlyDto = _mapper.Map<UserReadOnlyDto>(user);
                return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, userReadOnlyDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception POST in {nameof(PostUser)}");
                return StatusCode(500, Messages.Error500Message);
            }
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            _logger.LogInformation($"Request to {nameof(DeleteUser)} for ID: {id}");
            try
            {
                var user = await _context.User.FindAsync(id);
                if (user == null)
                {
                    _logger.LogWarning($"User with ID {id} not found in {nameof(DeleteUser)}");
                    return NotFound();
                }

                _context.User.Remove(user);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception DELETE in {nameof(DeleteUser)}");
                return StatusCode(500, Messages.Error500Message);
            }
        }

        private bool UserExists(int id)
        {
            return _context.User.Any(e => e.UserId == id);
        }



        // POST: api/Users/login

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == loginRequest.UserId && u.UserPassword == loginRequest.Password);

            if (user == null)
            {
                return Unauthorized();
            }

            // Check if the user is admin
            if (user.LocationId == 1)
            {
                user.LocationId = loginRequest.SelectedLocationId; // Use selected location for admin
            }

            UserSession.LoggedInUserId = user.UserId;
                
            await LogLogin(user.UserId);

            // Return user details, including the possibly updated LocationId for admin
            return Ok(new UserReadOnlyDto
            {
                UserId = user.UserId,
                UserName = user.UserName,
                LocationId = user.LocationId
            });
        }


        public static class UserSession
        {
            public static int LoggedInUserId { get; set; }
        }
        public class LoginRequest
        {
            public int UserId { get; set; }
            public string Password { get; set; }
            public int SelectedLocationId { get; set; }
        }

        private async Task LogLogin(int userId)
        {
            try
            {
                var dateTimeNowFormatted = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");


                UserSession.LoggedInUserId = 0;

                // Use direct SQL execution for logging
                var query = $"INSERT INTO Logs (UserId, Dates) VALUES ({userId}, '{dateTimeNowFormatted}')";
                await _context.Database.ExecuteSqlRawAsync(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred while logging login: {ex.Message}");
                throw; // Handle exception appropriately for your application
            }
        }

        // POST: api/Users/logout
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            // Perform any logout-related tasks here, such as clearing session/token, logging the logout event, etc.
            // For simplicity, let's assume a basic logout operation.

            // Optionally log the logout action
            await LogLogout(UserSession.LoggedInUserId); // Implement the LogLogout method similarly to LogLogin

            return Ok(new { Message = "Logout Successful" });
        }

        private async Task LogLogout(int userId)
        {
            try
            {
                var dateTimeNowFormatted = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                // Use direct SQL execution for logging
                var query = $"INSERT INTO Logs (UserId, Dates, OutTime) VALUES ({userId}, '{dateTimeNowFormatted}', '{dateTimeNowFormatted}')";
                await _context.Database.ExecuteSqlRawAsync(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred while logging logout: {ex.Message}");
                throw; // Handle exception appropriately for your application
            }
        }
    }
}

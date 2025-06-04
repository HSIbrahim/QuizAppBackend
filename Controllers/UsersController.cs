using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizAppBackend.DTOs; // Ensure this is present
using QuizAppBackend.Services;
using System.Security.Claims;
using System.Linq; // Ensure Linq is available for string.Join

namespace QuizAppBackend.Controllers
{
    [Authorize] // Kräver autentisering
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        // Fixed CS8603 on return type
        private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpGet("profile")]
        public async Task<IActionResult> GetUserProfile()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ErrorDto { Message = "Autentisering krävs." });

            var userProfile = await _userService.GetUserProfileAsync(userId);
            if (userProfile == null)
            {
                return NotFound(new ErrorDto { Message = "Användarprofil hittades inte." });
            }
            return Ok(userProfile);
        }

        [HttpGet("{username}")] // För att hämta profil för en specifik användare med användarnamn
        public async Task<IActionResult> GetUserProfileByUsername(string username)
        {
            var userProfile = await _userService.GetUserProfileByUsernameAsync(username);
            if (userProfile == null)
            {
                return NotFound(new ErrorDto { Message = "Användarprofil hittades inte." });
            }
            return Ok(userProfile);
        }

        // NEW ENDPOINT: Search users for friend requests
        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest(new ErrorDto { Message = "Sökfråga kan inte vara tom." });
            }

            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ErrorDto { Message = "Autentisering krävs." });

            var users = await _userService.SearchUsersAsync(query, userId); // Pass current userId to exclude self
            return Ok(users);
        }

        // NEW ENDPOINT: Update user profile
        [HttpPatch("profile")] // Or Put if replacing the entire profile
        public async Task<IActionResult> UpdateUserProfile([FromBody] UpdateProfileDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorDto { Message = "Ogiltig profildata.", Details = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)) });
            }

            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ErrorDto { Message = "Autentisering krävs." });

            var result = await _userService.UpdateUserProfileAsync(userId, updateDto);
            if (result.IsSuccess)
            {
                return Ok(new { Message = "Profil uppdaterad." });
            }
            return BadRequest(new ErrorDto { Message = "Kunde inte uppdatera profilen.", Details = result.ErrorMessage }); // Provide more specific error
        }

        // NEW ENDPOINT: Change password
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorDto { Message = "Ogiltig lösenordsdata.", Details = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)) });
            }

            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ErrorDto { Message = "Autentisering krävs." });

            var result = await _userService.ChangeUserPasswordAsync(userId, changePasswordDto);
            if (result.IsSuccess)
            {
                return Ok(new { Message = "Lösenord ändrat." });
            }
            return BadRequest(new ErrorDto { Message = "Kunde inte ändra lösenord.", Details = result.ErrorMessage }); // Provide more specific error
        }
    }
}
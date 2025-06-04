using Microsoft.AspNetCore.Mvc;
using QuizAppBackend.DTOs;
using QuizAppBackend.Services;

namespace QuizAppBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorDto { Message = "Ogiltig registreringsdata.", Details = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)) });
            }

            var result = await _authService.RegisterAsync(registerDto);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(new ErrorDto { Message = "Kunde inte registrera användare.", Details = string.Join(", ", result.Errors ?? Enumerable.Empty<string>()) });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorDto { Message = "Ogiltig inloggningsdata.", Details = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)) });
            }

            var result = await _authService.LoginAsync(loginDto);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return Unauthorized(new ErrorDto { Message = "Ogiltigt användarnamn eller lösenord.", Details = string.Join(", ", result.Errors ?? Enumerable.Empty<string>()) });
        }
    }
}
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using QuizAppBackend.DTOs;
using QuizAppBackend.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration; // Ensure this is present

namespace QuizAppBackend.Services
{
    public class AuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;

        public AuthService(UserManager<User> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            var userExists = await _userManager.FindByNameAsync(registerDto.Username);
            if (userExists != null)
            {
                return new AuthResponseDto { IsSuccess = false, Errors = new[] { "Användarnamn finns redan." } };
            }

            var newUser = new User
            {
                UserName = registerDto.Username,
                Email = registerDto.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                TotalScore = 0
            };

            var result = await _userManager.CreateAsync(newUser, registerDto.Password);
            if (!result.Succeeded)
            {
                return new AuthResponseDto { IsSuccess = false, Errors = result.Errors.Select(e => e.Description) };
            }

            return new AuthResponseDto { IsSuccess = true, UserId = newUser.Id, Username = newUser.UserName };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByNameAsync(loginDto.Username);
            if (user == null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                return new AuthResponseDto { IsSuccess = false, Errors = new[] { "Ogiltigt användarnamn eller lösenord." } };
            }

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName!), // Fixed CS8604: user.UserName should not be null for IdentityUser
                new Claim(ClaimTypes.NameIdentifier, user.Id), // Fixed CS8604: user.Id should not be null
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            // Fixed CS8604: Use null-forgiving operator as we expect these to be configured
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]!));

            // Fixed CS8604: Use null-forgiving operator as we expect these to be configured
            var issuer = _configuration["JwtSettings:Issuer"]!;
            var audience = _configuration["JwtSettings:Audience"]!;

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                expires: DateTime.Now.AddHours(3), // Token giltig i 3 timmar
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return new AuthResponseDto
            {
                IsSuccess = true,
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                UserId = user.Id,
                Username = user.UserName! // Fixed CS8601
            };
        }
    }
}
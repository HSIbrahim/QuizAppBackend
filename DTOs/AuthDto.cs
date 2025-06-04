using System.ComponentModel.DataAnnotations; // Added

namespace QuizAppBackend.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Användarnamn är obligatoriskt.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Användarnamn måste vara mellan 3 och 50 tecken.")]
        public string Username { get; set; } = string.Empty; // Fixed CS8618

        [Required(ErrorMessage = "E-post är obligatoriskt.")]
        [EmailAddress(ErrorMessage = "Ogiltig e-postadress.")]
        public string Email { get; set; } = string.Empty; // Fixed CS8618

        [Required(ErrorMessage = "Lösenord är obligatoriskt.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Lösenord måste vara minst 6 tecken.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty; // Fixed CS8618
    }

    public class LoginDto
    {
        [Required(ErrorMessage = "Användarnamn är obligatoriskt.")]
        public string Username { get; set; } = string.Empty; // Fixed CS8618

        [Required(ErrorMessage = "Lösenord är obligatoriskt.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty; // Fixed CS8618
    }

    public class AuthResponseDto
    {
        public string UserId { get; set; } = string.Empty; // Fixed CS8618
        public string Username { get; set; } = string.Empty; // Fixed CS8618
        public string Token { get; set; } = string.Empty; // Fixed CS8618
        public bool IsSuccess { get; set; }
        public IEnumerable<string> Errors { get; set; } = Enumerable.Empty<string>(); // Fixed CS8618 (Initialize to empty enumerable)
    }
}
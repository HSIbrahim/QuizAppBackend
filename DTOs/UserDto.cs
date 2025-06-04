using System.ComponentModel.DataAnnotations; // Added

namespace QuizAppBackend.DTOs
{
    public class UserProfileDto
    {
        public string UserId { get; set; } = string.Empty; // Fixed CS8618
        public string Username { get; set; } = string.Empty; // Fixed CS8618
        public string Email { get; set; } = string.Empty; // Fixed CS8618
        public int TotalScore { get; set; }
    }

    // NEW DTO: For updating user profile (e.g., username, email)
    public class UpdateProfileDto
    {
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Användarnamn måste vara mellan 3 och 50 tecken.")]
        public string? NewUsername { get; set; } // Fixed CS8618 (Nullable, as it's optional update)

        [EmailAddress(ErrorMessage = "Ogiltig e-postadress.")]
        public string? NewEmail { get; set; } // Fixed CS8618 (Nullable, as it's optional update)
    }

    // NEW DTO: For changing user password
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Nuvarande lösenord är obligatoriskt.")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty; // Fixed CS8618

        [Required(ErrorMessage = "Nytt lösenord är obligatoriskt.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Nytt lösenord måste vara minst 6 tecken långt.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty; // Fixed CS8618

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Lösenord och bekräftelselösenord matchar inte.")]
        public string ConfirmNewPassword { get; set; } = string.Empty; // Fixed CS8618
    }
}
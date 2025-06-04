using System.ComponentModel.DataAnnotations; // Added

namespace QuizAppBackend.DTOs
{
    public class FriendRequestDto
    {
        [Required(ErrorMessage = "Mottagarens användarnamn är obligatoriskt.")]
        public string ReceiverUsername { get; set; } = string.Empty; // Fixed CS8618
    }

    public class FriendDto
    {
        public string UserId { get; set; } = string.Empty; // Fixed CS8618
        public string Username { get; set; } = string.Empty; // Fixed CS8618
        public bool IsAccepted { get; set; }
        public bool IsSender { get; set; } // Indikerar om den aktuella användaren skickade förfrågan
    }
}
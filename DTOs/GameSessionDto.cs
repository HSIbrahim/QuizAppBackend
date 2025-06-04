using QuizAppBackend.Models;
using System.ComponentModel.DataAnnotations; // Added

namespace QuizAppBackend.DTOs
{
    public class CreateGameSessionDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Välj en giltig kategori.")]
        public int QuizCategoryId { get; set; }

        // Enums don't strictly need [Required] as they have default values,
        // but can be validated for specific ranges if needed.
        public DifficultyLevel Difficulty { get; set; }
    }

    public class GameSessionDetailsDto
    {
        public Guid Id { get; set; }
        public string HostUsername { get; set; } = string.Empty; // Fixed CS8618
        public int QuizCategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty; // Fixed CS8618
        public DifficultyLevel Difficulty { get; set; }
        public DateTime StartTime { get; set; }
        public bool IsActive { get; set; }
        public List<GameSessionPlayerDto> Players { get; set; } = new List<GameSessionPlayerDto>(); // Fixed CS8618 (Initialize collection)
    }

    public class GameSessionPlayerDto
    {
        public string UserId { get; set; } = string.Empty; // Fixed CS8618
        public string Username { get; set; } = string.Empty; // Fixed CS8618
        public int Score { get; set; }
        public bool IsHost { get; set; }
    }
}
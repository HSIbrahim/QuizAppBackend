using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json; // Added for JsonSerializer

namespace QuizAppBackend.Models
{
    public class GameSession
    {
        public Guid Id { get; set; } // Använd GUID för session-ID för enklare delning
        public string HostId { get; set; } = string.Empty; // Fixed CS8618
        public int QuizCategoryId { get; set; }
        public QuizCategory? QuizCategory { get; set; } // Fixed CS8618 (Navigation property can be null)
        public DifficultyLevel Difficulty { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool IsActive { get; set; } = true;

        // --- NEW PROPERTIES FOR GAME STATE ---
        // Lagrar listan av fråge-ID:n för sessionen som en JSON-sträng
        public string QuestionOrderJson { get; set; } = "[]"; // Fixed CS8618 (Initialize to empty JSON array)

        // Ej mappad till DB, används för enkel åtkomst till fråge-ID:n
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public List<int> QuestionOrder
        {
            get => QuestionOrderJson == null ? new List<int>() : JsonSerializer.Deserialize<List<int>>(QuestionOrderJson) ?? new List<int>(); // Added ?? new List<int>()
            set => QuestionOrderJson = JsonSerializer.Serialize(value);
        }

        public int CurrentQuestionIndex { get; set; } = 0; // Spårar vilken fråga som ska ställas nästa
        // --- END NEW PROPERTIES ---

        // Navigationsproperty för spelare i sessionen
        public ICollection<GameSessionPlayer> Players { get; set; } = new List<GameSessionPlayer>(); // Fixed CS8618 (Initialize collection)
    }
}
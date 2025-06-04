using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizAppBackend.Models
{
    public class ScoreEntry
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty; // Fixed CS8618
        public int Score { get; set; }
        public DateTime DateAchieved { get; set; }
        public int QuizCategoryId { get; set; } // Vilken kategori poängen kom från
        public DifficultyLevel Difficulty { get; set; } // Svårighetsgraden för quizet

        // Navigationsproperties
        public User? User { get; set; } // Fixed CS8618 (Navigation property can be null)
        public QuizCategory? QuizCategory { get; set; } // Fixed CS8618 (Navigation property can be null)
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Diagnostics.CodeAnalysis; // Added for DisallowNull

namespace QuizAppBackend.Models
{
    public enum DifficultyLevel
    {
        Easy,
        Medium,
        Hard
    }

    public enum QuestionType
    {
        MultipleChoice,
        TrueFalse
    }

    public class Question
    {
        public int Id { get; set; }

        // Marked as 'required' because these must have values when creating a Question
        public required string Text { get; set; }

        // Lagra svarsalternativen som en JSON-sträng för flexibilitet
        public required string OptionsJson { get; set; } // Marked as 'required'

        [NotMapped] // Indikerar att denna egenskap inte ska mappas till en databaskolumn
        public List<string> Options
        {
            // Ensure Deserialize<List<string>> doesn't return null by coalescing with new List<string>()
            get => OptionsJson == null ? new List<string>() : JsonSerializer.Deserialize<List<string>>(OptionsJson) ?? new List<string>();
            set => OptionsJson = JsonSerializer.Serialize(value);
        }

        public required string CorrectAnswer { get; set; } // Marked as 'required'

        public DifficultyLevel Difficulty { get; set; }
        public QuestionType Type { get; set; }

        // Foreign Key
        public int QuizCategoryId { get; set; }
        // Navigationsproperty - Marked as nullable because it might not be loaded initially
        public QuizCategory? Category { get; set; }

        // Navigationsproperty för användares svar - Initialized to an empty list to avoid null reference exceptions
        public ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>(); // Initialized
    }
}
using QuizAppBackend.Models;
using System.Collections.Generic; // Added for List
using System.ComponentModel.DataAnnotations; // Added for Required

namespace QuizAppBackend.DTOs
{
    public class QuestionDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty; // Fixed CS8618
        public List<string> Options { get; set; } = new List<string>(); // Fixed CS8618 (Initialize collection)
        public DifficultyLevel Difficulty { get; set; }
        public QuestionType Type { get; set; }
        public int QuizCategoryId { get; set; }
    }

    public class SubmitAnswerDto
    {
        public int QuestionId { get; set; }
        public string SubmittedAnswer { get; set; } = string.Empty; // Fixed CS8618
        public Guid GameSessionId { get; set; } // Om det är en online-session
    }

    public class AnswerResultDto
    {
        public bool IsCorrect { get; set; }
        public int PointsAwarded { get; set; }
        public int CurrentScore { get; set; }
    }
}
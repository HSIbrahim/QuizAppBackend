using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizAppBackend.Models
{
    public class UserAnswer
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty; // Fixed CS8618
        public int QuestionId { get; set; }
        public Guid GameSessionId { get; set; } // Kopplad till en specifik spelsession (om online)
        public string SubmittedAnswer { get; set; } = string.Empty; // Fixed CS8618
        public bool IsCorrect { get; set; }
        public int PointsAwarded { get; set; } = 0;
        public DateTime AnsweredAt { get; set; }

        // Navigationsproperties
        public User? User { get; set; } // Fixed CS8618 (Navigation property can be null)
        public Question? Question { get; set; } // Fixed CS8618 (Navigation property can be null)
        public GameSession? GameSession { get; set; } // Fixed CS8618 (Navigation property can be null)
        public int GameSessionPlayerId { get; set; }
        public GameSessionPlayer? GameSessionPlayer { get; set; } // Fixed CS8618 (Navigation property can be null)
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizAppBackend.Models
{
    public class GameSessionPlayer
    {
        public int Id { get; set; }
        public Guid GameSessionId { get; set; }
        public string UserId { get; set; } = string.Empty; // Fixed CS8618
        public int Score { get; set; } = 0;
        public bool IsHost { get; set; } = false; // Sant om spelaren är värd för sessionen
        public DateTime JoinedAt { get; set; }

        // Navigationsproperties
        public GameSession? GameSession { get; set; } // Fixed CS8618 (Navigation property can be null)
        public User? User { get; set; } // Fixed CS8618 (Navigation property can be null)

        // Navigationsproperty för användares svar i denna session
        public ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>(); // Fixed CS8618 (Initialize collection)
    }
}
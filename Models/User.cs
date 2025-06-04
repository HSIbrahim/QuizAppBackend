using Microsoft.AspNetCore.Identity;
using System.Collections.Generic; // Added for List

namespace QuizAppBackend.Models
{
    // Vi ärver från IdentityUser för att få inbyggd hantering av lösenord, roller, etc.
    public class User : IdentityUser
    {
        // Lägg till eventuella egna användarprofilfält här
        public int TotalScore { get; set; } = 0; // Global totalpoäng för användaren

        // Navigationsproperty för vänskapsrelationer
        public ICollection<Friendship> FriendshipsSent { get; set; } = new List<Friendship>(); // Fixed CS8618
        public ICollection<Friendship> FriendshipsReceived { get; set; } = new List<Friendship>(); // Fixed CS8618

        // Navigationsproperty för spelarsessioner
        public ICollection<GameSessionPlayer> GameSessionsPlayed { get; set; } = new List<GameSessionPlayer>(); // Fixed CS8618

        // Navigationsproperty för användares svar
        public ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>(); // Fixed CS8618

        // Navigationsproperty för poängposter
        public ICollection<ScoreEntry> ScoreEntries { get; set; } = new List<ScoreEntry>(); // Fixed CS8618
    }
}
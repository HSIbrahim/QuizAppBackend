namespace QuizAppBackend.DTOs
{
    public class LeaderboardEntryDto
    {
        public string Username { get; set; } = string.Empty; // Fixed CS8618
        public int TotalScore { get; set; }
    }
}
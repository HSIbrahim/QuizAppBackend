namespace QuizAppBackend.DTOs
{
    public class ErrorDto
    {
        public string Message { get; set; } = string.Empty; // Fixed CS8618
        public string? Details { get; set; } // Fixed CS8618 (Optional, can be null)
        public string? Code { get; set; } // Fixed CS8618 (Optional, can be null)
    }
}
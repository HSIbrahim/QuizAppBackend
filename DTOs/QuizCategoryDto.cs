namespace QuizAppBackend.DTOs
{
    public class QuizCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // Fixed CS8618
        public string Description { get; set; } = string.Empty; // Fixed CS8618
    }
}
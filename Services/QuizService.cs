using Microsoft.EntityFrameworkCore;
using QuizAppBackend.Data;
using QuizAppBackend.Models;
using QuizAppBackend.DTOs;

namespace QuizAppBackend.Services
{
    public class QuizService
    {
        private readonly ApplicationDbContext _context;

        public QuizService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<QuizCategoryDto>> GetQuizCategoriesAsync()
        {
            return await _context.QuizCategories
                .Select(c => new QuizCategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description
                })
                .ToListAsync();
        }

        public async Task<List<QuestionDto>> GetQuestionsForQuizAsync(int categoryId, DifficultyLevel difficulty, int count = 10)
        {
            var questions = await _context.Questions
                .Where(q => q.QuizCategoryId == categoryId && q.Difficulty == difficulty)
                .OrderBy(q => Guid.NewGuid()) // Slumpmässig ordning
                .Take(count)
                .Select(q => new QuestionDto
                {
                    Id = q.Id,
                    Text = q.Text,
                    Options = q.Options,
                    Difficulty = q.Difficulty,
                    Type = q.Type,
                    QuizCategoryId = q.QuizCategoryId
                })
                .ToListAsync();

            return questions;
        }

        public async Task<Question?> GetQuestionByIdAsync(int questionId) // Fixed CS8603
        {
            return await _context.Questions.FindAsync(questionId);
        }

        // NEW METHOD: Get QuestionDto by ID (used by GameService for SignalR)
        public async Task<QuestionDto?> GetQuestionDtoByIdAsync(int questionId) // Fixed CS8603
        {
            var question = await _context.Questions
                .Where(q => q.Id == questionId)
                .Select(q => new QuestionDto
                {
                    Id = q.Id,
                    Text = q.Text,
                    Options = q.Options,
                    Difficulty = q.Difficulty,
                    Type = q.Type,
                    QuizCategoryId = q.QuizCategoryId
                })
                .FirstOrDefaultAsync();

            return question;
        }


        public int CalculatePoints(DifficultyLevel difficulty)
        {
            return difficulty switch
            {
                DifficultyLevel.Easy => 10,
                DifficultyLevel.Medium => 20,
                DifficultyLevel.Hard => 30,
                _ => 10
            };
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizAppBackend.DTOs;
using QuizAppBackend.Models;
using QuizAppBackend.Services;
using System.Security.Claims;
using System.Linq; // Ensure Linq is available for string.Join

namespace QuizAppBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuizController : ControllerBase
    {
        private readonly QuizService _quizService;
        private readonly UserService _userService;

        public QuizController(QuizService quizService, UserService userService)
        {
            _quizService = quizService;
            _userService = userService;
        }

        // Fixed CS8603 on return type
        private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpGet("categories")]
        public async Task<IActionResult> GetQuizCategories()
        {
            var categories = await _quizService.GetQuizCategoriesAsync();
            return Ok(categories);
        }

        [Authorize] // Kräver autentisering för att hämta frågor (för solo-quiz)
        [HttpGet("questions")]
        public async Task<IActionResult> GetQuizQuestions(int categoryId, DifficultyLevel difficulty, int count = 10)
        {
            var questions = await _quizService.GetQuestionsForQuizAsync(categoryId, difficulty, count);
            if (questions == null || !questions.Any())
            {
                return NotFound(new ErrorDto { Message = "Inga frågor hittades för vald kategori och svårighetsgrad." });
            }
            return Ok(questions);
        }

        [Authorize]
        [HttpPost("submit-solo-answer")]
        public async Task<IActionResult> SubmitSoloAnswer([FromBody] SubmitAnswerDto submitAnswerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorDto { Message = "Ogiltig svarsdata.", Details = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)) });
            }

            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ErrorDto { Message = "Autentisering krävs." });

            var question = await _quizService.GetQuestionByIdAsync(submitAnswerDto.QuestionId);
            if (question == null) return NotFound(new ErrorDto { Message = "Frågan hittades inte." });

            var isCorrect = string.Equals(question.CorrectAnswer, submitAnswerDto.SubmittedAnswer, StringComparison.OrdinalIgnoreCase);
            var pointsAwarded = isCorrect ? _quizService.CalculatePoints(question.Difficulty) : 0;

            await _userService.UpdateUserScoreAndAddScoreEntry(
                userId,
                pointsAwarded,
                question.QuizCategoryId,
                question.Difficulty
            );

            var updatedUser = await _userService.GetUserProfileAsync(userId);

            return Ok(new AnswerResultDto
            {
                IsCorrect = isCorrect,
                PointsAwarded = pointsAwarded,
                CurrentScore = updatedUser?.TotalScore ?? 0
            });
        }
    }
}
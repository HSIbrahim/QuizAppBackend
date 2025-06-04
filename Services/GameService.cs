using Microsoft.EntityFrameworkCore;
using QuizAppBackend.Data;
using QuizAppBackend.DTOs;
using QuizAppBackend.Models;
using Microsoft.AspNetCore.Identity;
using System.Text.Json; // Added for JsonSerializer

namespace QuizAppBackend.Services
{
    public class GameService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly QuizService _quizService; // Använd QuizService för att hämta frågor

        public GameService(ApplicationDbContext context, UserManager<User> userManager, QuizService quizService)
        {
            _context = context;
            _userManager = userManager;
            _quizService = quizService;
        }

        public async Task<GameSessionDetailsDto?> CreateGameSessionAsync(string hostId, CreateGameSessionDto createDto) // Fixed CS8603
        {
            var host = await _userManager.FindByIdAsync(hostId);
            if (host == null) return null;

            var category = await _context.QuizCategories.FindAsync(createDto.QuizCategoryId);
            if (category == null) return null;

            var newSession = new GameSession
            {
                Id = Guid.NewGuid(),
                HostId = hostId,
                QuizCategoryId = createDto.QuizCategoryId,
                Difficulty = createDto.Difficulty,
                StartTime = DateTime.UtcNow,
                IsActive = true,
                CurrentQuestionIndex = 0 // Initialize question index
            };

            var hostPlayer = new GameSessionPlayer
            {
                UserId = hostId,
                Score = 0,
                IsHost = true,
                JoinedAt = DateTime.UtcNow
            };
            newSession.Players.Add(hostPlayer); // Changed assignment to Add to use initialized Players collection

            _context.GameSessions.Add(newSession);
            await _context.SaveChangesAsync();

            return new GameSessionDetailsDto
            {
                Id = newSession.Id,
                HostUsername = host.UserName ?? "Okänd", // Fixed CS8601, CS8618
                QuizCategoryId = newSession.QuizCategoryId,
                CategoryName = category.Name, // Fixed CS8618
                Difficulty = newSession.Difficulty,
                StartTime = newSession.StartTime,
                IsActive = newSession.IsActive,
                Players = new List<GameSessionPlayerDto>
                {
                    new GameSessionPlayerDto { UserId = host.Id, Username = host.UserName ?? "Okänd", Score = 0, IsHost = true } // Fixed CS8601
                }
            };
        }

        public async Task<GameSessionDetailsDto?> GetGameSessionDetailsAsync(Guid sessionId) // Fixed CS8603
        {
            var session = await _context.GameSessions
                .Include(gs => gs.QuizCategory)
                .Include(gs => gs.Players)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(gs => gs.Id == sessionId);

            if (session == null) return null;

            var hostUsername = session.Players.FirstOrDefault(p => p.IsHost)?.User?.UserName ?? "Okänd"; // Fixed CS8601

            return new GameSessionDetailsDto
            {
                Id = session.Id,
                HostUsername = hostUsername,
                QuizCategoryId = session.QuizCategoryId,
                CategoryName = session.QuizCategory?.Name ?? "Okänd Kategori", // Fixed CS8618, CS8601
                Difficulty = session.Difficulty,
                StartTime = session.StartTime,
                IsActive = session.IsActive,
                Players = session.Players.Select(p => new GameSessionPlayerDto
                {
                    UserId = p.UserId,
                    Username = p.User?.UserName ?? "Okänd", // Fixed CS8601
                    Score = p.Score,
                    IsHost = p.IsHost
                }).ToList()
            };
        }

        public async Task<bool> JoinGameSessionAsync(Guid sessionId, string userId)
        {
            var session = await _context.GameSessions
                .Include(gs => gs.Players)
                .FirstOrDefaultAsync(gs => gs.Id == sessionId && gs.IsActive);

            if (session == null) return false;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            // Kontrollera om användaren redan är med i sessionen
            if (session.Players.Any(p => p.UserId == userId))
            {
                return true; // Redan ansluten
            }

            var newPlayer = new GameSessionPlayer
            {
                GameSessionId = sessionId,
                UserId = userId,
                Score = 0,
                IsHost = false,
                JoinedAt = DateTime.UtcNow
            };

            session.Players.Add(newPlayer);
            await _context.SaveChangesAsync();
            return true;
        }

        // NEW METHOD: Initialize questions and start the game formally
        public async Task<(bool success, string? errorMessage)> StartGameSessionAsync(Guid gameSessionId) // Fixed CS8619
        {
            var session = await _context.GameSessions
                .FirstOrDefaultAsync(gs => gs.Id == gameSessionId);

            if (session == null)
            {
                return (false, "Spelsession hittades inte.");
            }
            if (!session.IsActive)
            {
                return (false, "Spelsessionen är inte aktiv.");
            }
            if (!string.IsNullOrEmpty(session.QuestionOrderJson) && session.QuestionOrderJson != "[]") // Check if questions are already set
            {
                return (false, "Spelet har redan startat.");
            }

            // Hämta frågor för sessionen
            var questions = await _quizService.GetQuestionsForQuizAsync(session.QuizCategoryId, session.Difficulty, 10); // Hämta 10 frågor

            if (questions == null || !questions.Any())
            {
                return (false, "Inga frågor hittades för denna kategori/svårighetsgrad.");
            }

            session.QuestionOrder = questions.Select(q => q.Id).ToList(); // Store ordered question IDs
            session.CurrentQuestionIndex = 0; // Reset or ensure it's 0 at start
            await _context.SaveChangesAsync();

            return (true, null);
        }

        public async Task EndGameSessionAsync(Guid sessionId)
        {
            var session = await _context.GameSessions.FindAsync(sessionId);
            if (session == null) return;

            session.IsActive = false;
            session.EndTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Spara slutpoäng för varje spelare som ScoreEntry
            var players = await _context.GameSessionPlayers
                                .Where(p => p.GameSessionId == sessionId)
                                .ToListAsync();

            foreach (var player in players)
            {
                var user = await _userManager.FindByIdAsync(player.UserId);
                if (user != null)
                {
                    user.TotalScore += player.Score; // Uppdatera användarens totala poäng
                    await _userManager.UpdateAsync(user);

                    _context.ScoreEntries.Add(new ScoreEntry
                    {
                        UserId = player.UserId,
                        Score = player.Score,
                        DateAchieved = DateTime.UtcNow,
                        QuizCategoryId = session.QuizCategoryId,
                        Difficulty = session.Difficulty
                    });
                }
            }
            await _context.SaveChangesAsync();
        }

        // Metod för att hantera svar och uppdatera poäng (används av GameHub)
        public async Task<(bool isCorrect, int pointsAwarded, int currentScore)> ProcessAnswerAsync(string userId, Guid gameSessionId, int questionId, string submittedAnswer)
        {
            var question = await _quizService.GetQuestionByIdAsync(questionId);
            if (question == null) return (false, 0, 0);

            var isCorrect = string.Equals(question.CorrectAnswer, submittedAnswer, StringComparison.OrdinalIgnoreCase);
            var pointsAwarded = isCorrect ? _quizService.CalculatePoints(question.Difficulty) : 0;

            var player = await _context.GameSessionPlayers
                                .FirstOrDefaultAsync(p => p.GameSessionId == gameSessionId && p.UserId == userId);

            if (player == null) return (false, 0, 0);

            player.Score += pointsAwarded;
            await _context.SaveChangesAsync();

            // Logga användarens svar
            var gameSessionPlayer = await _context.GameSessionPlayers
                .FirstOrDefaultAsync(gsp => gsp.GameSessionId == gameSessionId && gsp.UserId == userId);

            // Fixed CS8602: Check if gameSessionPlayer is null before dereferencing
            if (gameSessionPlayer == null)
            {
                // This scenario should ideally not happen if player was found above, but for safety:
                return (false, 0, player.Score);
            }

            _context.UserAnswers.Add(new UserAnswer
            {
                UserId = userId,
                QuestionId = questionId,
                GameSessionId = gameSessionId,
                SubmittedAnswer = submittedAnswer,
                IsCorrect = isCorrect,
                PointsAwarded = pointsAwarded,
                AnsweredAt = DateTime.UtcNow,
                GameSessionPlayerId = gameSessionPlayer.Id
            });
            await _context.SaveChangesAsync();

            return (isCorrect, pointsAwarded, player.Score);
        }

        // NEW METHOD: Get the next question for a game session
        public async Task<(QuestionDto? question, int currentQuestionNumber, bool isLastQuestion)> GetNextQuestionAsync(Guid gameSessionId) // Fixed CS8619
        {
            var session = await _context.GameSessions
                .FirstOrDefaultAsync(gs => gs.Id == gameSessionId);

            if (session == null || !session.IsActive || session.QuestionOrder == null || !session.QuestionOrder.Any())
            {
                return (null, 0, true); // Session invalid or no questions
            }

            if (session.CurrentQuestionIndex >= session.QuestionOrder.Count)
            {
                return (null, session.CurrentQuestionIndex, true); // No more questions
            }

            var nextQuestionId = session.QuestionOrder[session.CurrentQuestionIndex];
            var question = await _quizService.GetQuestionDtoByIdAsync(nextQuestionId); // Use a new method to get DTO

            if (question == null)
            {
                // Handle case where question ID is in list but question doesn't exist
                return (null, session.CurrentQuestionIndex, true);
            }

            // Increment index for next call
            session.CurrentQuestionIndex++;
            await _context.SaveChangesAsync();

            bool isLastQuestion = session.CurrentQuestionIndex >= session.QuestionOrder.Count;

            return (question, session.CurrentQuestionIndex, isLastQuestion);
        }
    }
}
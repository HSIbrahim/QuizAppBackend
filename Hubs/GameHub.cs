using Microsoft.AspNetCore.SignalR;
using QuizAppBackend.DTOs;
using QuizAppBackend.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization; // Added for [Authorize]

namespace QuizAppBackend.Hubs
{
    // Kräver autentisering för att ansluta till hubben
    [Authorize] // Re-enable [Authorize] now that Program.cs handles JWT for SignalR
    public class GameHub : Hub
    {
        private readonly GameService _gameService;
        private readonly QuizService _quizService; // Keep for now if needed for specific hub logic
        private readonly UserService _userService; // Keep for now if needed for specific hub logic

        public GameHub(GameService gameService, QuizService quizService, UserService userService)
        {
            _gameService = gameService;
            _quizService = quizService;
            _userService = userService;
        }

        private string? GetUserId() // Fixed CS8603
        {
            // Hämta användarens ID från JWT-token (om autentiserad)
            return Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public async Task JoinGame(string gameSessionIdString)
        {
            Guid gameSessionId = Guid.Parse(gameSessionIdString);
            string? userId = GetUserId(); // Fixed CS8600

            if (string.IsNullOrEmpty(userId))
            {
                // Användaren är inte autentiserad, skicka fel eller koppla bort
                await Clients.Caller.SendAsync("ReceiveError", "Ej autentiserad. Vänligen logga in igen.");
                Context.Abort(); // Disconnect
                return;
            }

            var joined = await _gameService.JoinGameSessionAsync(gameSessionId, userId);

            if (joined)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, gameSessionId.ToString());

                // Skicka uppdaterade spelardetaljer till alla i gruppen
                var sessionDetails = await _gameService.GetGameSessionDetailsAsync(gameSessionId);
                if (sessionDetails != null)
                {
                    await Clients.Group(gameSessionId.ToString()).SendAsync("PlayerJoined", sessionDetails.Players);
                    await Clients.Caller.SendAsync("GameJoined", sessionDetails); // Skicka fulla detaljer till den som gick med
                }
                else
                {
                    await Clients.Caller.SendAsync("ReceiveError", "Kunde inte hämta spelsession efter anslutning.");
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameSessionId.ToString());
                }
            }
            else
            {
                await Clients.Caller.SendAsync("ReceiveError", "Kunde inte ansluta till spelet.");
            }
        }

        public async Task StartGame(string gameSessionIdString)
        {
            Guid gameSessionId = Guid.Parse(gameSessionIdString);
            // string userId = GetUserId(); // Not directly used here, but implicitly from Context.User

            var sessionDetails = await _gameService.GetGameSessionDetailsAsync(gameSessionId);
            // Fixed CS8602: Check Context.User?.Identity before dereferencing Name
            if (sessionDetails == null || sessionDetails.HostUsername != Context.User?.Identity?.Name)
            {
                await Clients.Caller.SendAsync("ReceiveError", "Endast värden kan starta spelet.");
                return;
            }

            // Call GameService to initialize and persist game questions
            var (success, errorMessage) = await _gameService.StartGameSessionAsync(gameSessionId);

            if (!success)
            {
                await Clients.Caller.SendAsync("ReceiveError", errorMessage);
                return;
            }

            await Clients.Group(gameSessionId.ToString()).SendAsync("GameStarted");
            await SendNextQuestion(gameSessionId);
        }

        public async Task SubmitAnswer(string gameSessionIdString, SubmitAnswerDto submitAnswerDto)
        {
            Guid gameSessionId = Guid.Parse(gameSessionIdString);
            string? userId = GetUserId(); // Fixed CS8600

            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.SendAsync("ReceiveError", "Ej autentiserad.");
                return;
            }

            // Bearbeta svaret via GameService
            var (isCorrect, pointsAwarded, currentScore) = await _gameService.ProcessAnswerAsync(userId, gameSessionId, submitAnswerDto.QuestionId, submitAnswerDto.SubmittedAnswer);

            // Skicka resultat till den som svarade
            await Clients.Caller.SendAsync("AnswerResult", new AnswerResultDto
            {
                IsCorrect = isCorrect,
                PointsAwarded = pointsAwarded,
                CurrentScore = currentScore
            });

            // Skicka uppdaterade poäng till alla i gruppen
            var player = (await _gameService.GetGameSessionDetailsAsync(gameSessionId))?.Players.FirstOrDefault(p => p.UserId == userId);
            if (player != null)
            {
                await Clients.Group(gameSessionId.ToString()).SendAsync("PlayerScoreUpdated", player.Username, player.Score);
            }
        }

        public async Task RequestNextQuestion(string gameSessionIdString) // Renamed from NextQuestion to avoid confusion
        {
            Guid gameSessionId = Guid.Parse(gameSessionIdString);
            // string userId = GetUserId(); // Not directly used here, but implicitly from Context.User

            // Endast värden eller servern ska kunna be om nästa fråga
            var sessionDetails = await _gameService.GetGameSessionDetailsAsync(gameSessionId);
            // Fixed CS8602: Check Context.User?.Identity before dereferencing Name
            if (sessionDetails == null || sessionDetails.HostUsername != Context.User?.Identity?.Name)
            {
                await Clients.Caller.SendAsync("ReceiveError", "Endast värden kan be om nästa fråga.");
                return;
            }

            await SendNextQuestion(gameSessionId);
        }

        private async Task SendNextQuestion(Guid gameSessionId)
        {
            // Get next question using GameService
            var (question, currentQuestionNumber, isLastQuestion) = await _gameService.GetNextQuestionAsync(gameSessionId);

            if (question == null)
            {
                // No more questions or error getting question
                await Clients.Group(gameSessionId.ToString()).SendAsync("GameOver");
                await _gameService.EndGameSessionAsync(gameSessionId);
                return;
            }

            await Clients.Group(gameSessionId.ToString()).SendAsync("ReceiveQuestion", question, currentQuestionNumber, isLastQuestion);
        }

        public override async Task OnDisconnectedAsync(Exception? exception) // Fixed CS8765
        {
            // Handle if a player disconnects from an active session
            // This can get complex in a real app (e.g., if the host disconnects)
            // For simplicity, ignore for now
            // You might want to update GameSessionPlayers' status or remove them if needed.
            await base.OnDisconnectedAsync(exception);
        }
    }
}
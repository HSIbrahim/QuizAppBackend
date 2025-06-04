using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizAppBackend.DTOs;
using QuizAppBackend.Hubs;
using QuizAppBackend.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR; // För att kunna skicka meddelanden via hubben
using System.Net; // Added for HttpStatusCode

namespace QuizAppBackend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        private readonly GameService _gameService;
        private readonly IHubContext<GameHub> _gameHubContext; // För att skicka meddelanden utanför Hub-metoder

        public GameController(GameService gameService, IHubContext<GameHub> gameHubContext)
        {
            _gameService = gameService;
            _gameHubContext = gameHubContext;
        }

        // Fixed CS8603 on return type
        private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpPost("create")]
        public async Task<IActionResult> CreateGameSession([FromBody] CreateGameSessionDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorDto { Message = "Ogiltig spelsessionsdata.", Details = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)) });
            }

            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ErrorDto { Message = "Autentisering krävs." });

            var session = await _gameService.CreateGameSessionAsync(userId, createDto);
            if (session == null)
            {
                return BadRequest(new ErrorDto { Message = "Kunde inte skapa spelsession.", Details = "Kategori hittades inte eller användare ogiltig." });
            }

            // Skicka meddelande till alla i hubben att en ny session skapats
            // (Om vi hade en "lobby" för att lista aktiva spel)
            // await _gameHubContext.Clients.All.SendAsync("NewGameSessionCreated", session);

            return Ok(session);
        }

        [HttpGet("{sessionId}")]
        public async Task<IActionResult> GetGameSessionDetails(Guid sessionId)
        {
            var session = await _gameService.GetGameSessionDetailsAsync(sessionId);
            if (session == null)
            {
                return NotFound(new ErrorDto { Message = "Spelsession hittades inte." });
            }
            return Ok(session);
        }

        [HttpPost("{sessionId}/end")]
        public async Task<IActionResult> EndGameSession(Guid sessionId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ErrorDto { Message = "Autentisering krävs." });

            var session = await _gameService.GetGameSessionDetailsAsync(sessionId);
            if (session == null)
            {
                return NotFound(new ErrorDto { Message = "Spelsession hittades inte." });
            }

            // Fixed CS8602: User.Identity.Name can be null.
            if (session.HostUsername != User.Identity?.Name) // Check for null Identity or Name
            {
                return StatusCode((int)HttpStatusCode.Forbidden, new ErrorDto { Message = "Du har inte behörighet att avsluta detta spel." });
            }

            await _gameService.EndGameSessionAsync(sessionId);

            // Meddela spelare via SignalR att spelet är slut
            await _gameHubContext.Clients.Group(sessionId.ToString()).SendAsync("GameOver");

            return Ok(new { Message = "Spelsession avslutad." });
        }
    }
}
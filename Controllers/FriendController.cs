using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizAppBackend.DTOs;
using QuizAppBackend.Services;
using System.Security.Claims;
using System.Linq; // Ensure Linq is available for string.Join

namespace QuizAppBackend.Controllers
{
    [Authorize] // Kräver autentisering för alla endpoints i denna controller
    [ApiController]
    [Route("api/[controller]")]
    public class FriendController : ControllerBase
    {
        private readonly FriendService _friendService;

        public FriendController(FriendService friendService)
        {
            _friendService = friendService;
        }

        // Fixed CS8603 on return type
        private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpPost("send-request")]
        public async Task<IActionResult> SendFriendRequest([FromBody] FriendRequestDto requestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorDto { Message = "Ogiltig förfrågningsdata.", Details = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)) });
            }

            var senderId = GetUserId();
            if (senderId == null) return Unauthorized(new ErrorDto { Message = "Autentisering krävs." });

            var success = await _friendService.SendFriendRequestAsync(senderId, requestDto.ReceiverUsername);
            if (success)
            {
                return Ok(new { Message = "Vänförfrågan skickad." });
            }
            return BadRequest(new ErrorDto { Message = "Kunde inte skicka vänförfrågan.", Details = "Användare hittades inte, förfrågan finns redan eller är redan vänner." });
        }

        [HttpPost("accept-request/{senderId}")]
        public async Task<IActionResult> AcceptFriendRequest(string senderId)
        {
            var currentUserId = GetUserId();
            if (currentUserId == null) return Unauthorized(new ErrorDto { Message = "Autentisering krävs." });

            var success = await _friendService.AcceptFriendRequestAsync(currentUserId, senderId);
            if (success)
            {
                return Ok(new { Message = "Vänförfrågan accepterad." });
            }
            return BadRequest(new ErrorDto { Message = "Kunde inte acceptera vänförfrågan.", Details = "Förfrågan hittades inte eller är redan accepterad." });
        }

        [HttpPost("reject-request/{senderId}")]
        public async Task<IActionResult> RejectFriendRequest(string senderId)
        {
            var currentUserId = GetUserId();
            if (currentUserId == null) return Unauthorized(new ErrorDto { Message = "Autentisering krävs." });

            var success = await _friendService.RejectFriendRequestAsync(currentUserId, senderId);
            if (success)
            {
                return Ok(new { Message = "Vänförfrågan nekad." });
            }
            return BadRequest(new ErrorDto { Message = "Kunde inte neka vänförfrågan.", Details = "Förfrågan hittades inte eller är redan accepterad." });
        }

        [HttpDelete("remove-friend/{friendId}")]
        public async Task<IActionResult> RemoveFriend(string friendId)
        {
            var currentUserId = GetUserId();
            if (currentUserId == null) return Unauthorized(new ErrorDto { Message = "Autentisering krävs." });

            var success = await _friendService.RemoveFriendAsync(currentUserId, friendId);
            if (success)
            {
                return Ok(new { Message = "Vän borttagen." });
            }
            return BadRequest(new ErrorDto { Message = "Kunde inte ta bort vän.", Details = "Relation hittades inte." });
        }

        [HttpGet("my-friends")]
        public async Task<IActionResult> GetMyFriends()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ErrorDto { Message = "Autentisering krävs." });

            var friends = await _friendService.GetFriendsAsync(userId);
            return Ok(friends);
        }

        [HttpGet("pending-requests")]
        public async Task<IActionResult> GetPendingRequests()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ErrorDto { Message = "Autentisering krävs." });

            var pendingRequests = await _friendService.GetPendingFriendRequestsAsync(userId);
            return Ok(pendingRequests);
        }
    }
}
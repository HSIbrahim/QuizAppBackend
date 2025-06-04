using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizAppBackend.Services;

namespace QuizAppBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaderboardController : ControllerBase
    {
        private readonly LeaderboardService _leaderboardService;

        public LeaderboardController(LeaderboardService leaderboardService)
        {
            _leaderboardService = leaderboardService;
        }

        [HttpGet("global")]
        public async Task<IActionResult> GetGlobalLeaderboard()
        {
            var leaderboard = await _leaderboardService.GetGlobalLeaderboardAsync();
            return Ok(leaderboard);
        }

        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetCategoryLeaderboard(int categoryId)
        {
            var leaderboard = await _leaderboardService.GetCategoryLeaderboardAsync(categoryId);
            return Ok(leaderboard);
        }
    }
}
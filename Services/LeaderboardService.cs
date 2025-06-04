using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuizAppBackend.Data;
using QuizAppBackend.DTOs;
using QuizAppBackend.Models;
using System.Linq; // Added for Linq

namespace QuizAppBackend.Services
{
    public class LeaderboardService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public LeaderboardService(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<List<LeaderboardEntryDto>> GetGlobalLeaderboardAsync(int topN = 100)
        {
            var leaderboard = await _userManager.Users
                .OrderByDescending(u => u.TotalScore)
                .Take(topN)
                .Select(u => new LeaderboardEntryDto
                {
                    Username = u.UserName!, // Fixed CS8601
                    TotalScore = u.TotalScore
                })
                .ToListAsync();

            return leaderboard;
        }

        public async Task<List<LeaderboardEntryDto>> GetCategoryLeaderboardAsync(int categoryId, int topN = 100)
        {
            // Detta kräver aggregering av ScoreEntries per användare och kategori
            var leaderboard = await _context.ScoreEntries
                .Where(se => se.QuizCategoryId == categoryId)
                .GroupBy(se => se.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    CategoryScore = g.Sum(se => se.Score)
                })
                .OrderByDescending(x => x.CategoryScore)
                .Take(topN)
                .Join(_userManager.Users,
                      scoreEntry => scoreEntry.UserId,
                      user => user.Id,
                      (scoreEntry, user) => new LeaderboardEntryDto
                      {
                          Username = user.UserName!, // Fixed CS8601
                          TotalScore = scoreEntry.CategoryScore
                      })
                .ToListAsync();

            return leaderboard;
        }
    }
}
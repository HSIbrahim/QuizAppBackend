using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuizAppBackend.Data;
using QuizAppBackend.DTOs;
using QuizAppBackend.Models;
using System;
using System.Collections.Generic; // Added for List
using System.Linq; // Added for Linq

namespace QuizAppBackend.Services
{
    // NEW HELPER CLASS FOR SERVICE RESULTS
    public class ServiceResult
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; } // Fixed CS8618, CS8625
        public IEnumerable<string>? Errors { get; set; } // Fixed CS8618, CS8625

        public static ServiceResult Success() => new ServiceResult { IsSuccess = true };
        public static ServiceResult Failure(string errorMessage, IEnumerable<string>? errors = null) => new ServiceResult { IsSuccess = false, ErrorMessage = errorMessage, Errors = errors };
    }

    public class UserService // MÅSTE vara public
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public UserService(UserManager<User> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // Metod för att hämta användarprofil
        public async Task<UserProfileDto?> GetUserProfileAsync(string userId) // Fixed CS8603
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            return new UserProfileDto
            {
                UserId = user.Id,
                Username = user.UserName!, // Fixed CS8601, CS8602
                Email = user.Email!, // Fixed CS8601, CS8602
                TotalScore = user.TotalScore
            };
        }

        // Metod för att hämta användarprofil baserat på användarnamn (även denna bör vara public)
        public async Task<UserProfileDto?> GetUserProfileByUsernameAsync(string username) // Fixed CS8603
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null) return null;

            return new UserProfileDto
            {
                UserId = user.Id,
                Username = user.UserName!, // Fixed CS8601, CS8602
                Email = user.Email!, // Fixed CS8601, CS8602
                TotalScore = user.TotalScore
            };
        }

        // NEW METHOD: Search users
        public async Task<List<UserProfileDto>> SearchUsersAsync(string query, string currentUserId)
        {
            var users = await _userManager.Users
                .Where(u => u.UserName != null && u.UserName.Contains(query) && u.Id != currentUserId) // Ensure UserName is not null before Contains
                .Select(u => new UserProfileDto
                {
                    UserId = u.Id,
                    Username = u.UserName!, // Fixed CS8601
                    TotalScore = u.TotalScore // You might want to limit exposed fields for search
                })
                .ToListAsync();

            return users;
        }

        // NEW METHOD: Update user profile
        public async Task<ServiceResult> UpdateUserProfileAsync(string userId, UpdateProfileDto updateDto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return ServiceResult.Failure("Användare hittades inte.");
            }

            bool changed = false;

            if (!string.IsNullOrWhiteSpace(updateDto.NewUsername) && user.UserName != updateDto.NewUsername)
            {
                var existingUserWithNewUsername = await _userManager.FindByNameAsync(updateDto.NewUsername);
                if (existingUserWithNewUsername != null && existingUserWithNewUsername.Id != user.Id)
                {
                    return ServiceResult.Failure("Användarnamnet är redan upptaget.");
                }
                user.UserName = updateDto.NewUsername;
                changed = true;
            }

            if (!string.IsNullOrWhiteSpace(updateDto.NewEmail) && user.Email != updateDto.NewEmail)
            {
                var existingUserWithNewEmail = await _userManager.FindByEmailAsync(updateDto.NewEmail);
                if (existingUserWithNewEmail != null && existingUserWithNewEmail.Id != user.Id)
                {
                    return ServiceResult.Failure("E-postadressen är redan upptagen.");
                }
                user.Email = updateDto.NewEmail;
                changed = true;
            }

            if (changed)
            {
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    return ServiceResult.Failure("Kunde inte uppdatera profilen.", result.Errors.Select(e => e.Description));
                }
            }

            return ServiceResult.Success();
        }

        // NEW METHOD: Change user password
        public async Task<ServiceResult> ChangeUserPasswordAsync(string userId, ChangePasswordDto changePasswordDto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return ServiceResult.Failure("Användare hittades inte.");
            }

            var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
            if (!result.Succeeded)
            {
                return ServiceResult.Failure("Kunde inte ändra lösenord.", result.Errors.Select(e => e.Description));
            }
            return ServiceResult.Success();
        }


        // Metod för att uppdatera användarens poäng och lägga till en ScoreEntry
        public async Task UpdateUserScoreAndAddScoreEntry(string userId, int pointsAwarded, int quizCategoryId, DifficultyLevel difficulty) // MÅSTE vara public
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.TotalScore += pointsAwarded;
                await _userManager.UpdateAsync(user); // Uppdatera användarens totala poäng

                // Logga poängen som en ScoreEntry
                _context.ScoreEntries.Add(new ScoreEntry
                {
                    UserId = userId,
                    Score = pointsAwarded,
                    DateAchieved = DateTime.UtcNow,
                    QuizCategoryId = quizCategoryId,
                    Difficulty = difficulty
                });
                await _context.SaveChangesAsync(); // Spara ScoreEntry
            }
        }
    }
}
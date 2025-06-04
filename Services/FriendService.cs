using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuizAppBackend.Data;
using QuizAppBackend.DTOs;
using QuizAppBackend.Models;
using System.Linq; // Added for Linq

namespace QuizAppBackend.Services
{
    public class FriendService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public FriendService(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<bool> SendFriendRequestAsync(string senderId, string receiverUsername)
        {
            var receiver = await _userManager.FindByNameAsync(receiverUsername);
            if (receiver == null || receiver.Id == senderId)
            {
                return false; // Mottagare finns inte eller är samma användare
            }

            // Kolla om förfrågan redan finns (båda riktningar)
            var existingRequest = await _context.Friendships
                .FirstOrDefaultAsync(f =>
                    (f.SenderId == senderId && f.ReceiverId == receiver.Id) ||
                    (f.SenderId == receiver.Id && f.ReceiverId == senderId && f.Accepted == false)); // Kolla om mottagaren har skickat en icke-accepterad förfrågan

            if (existingRequest != null)
            {
                if (!existingRequest.Accepted && existingRequest.ReceiverId == senderId)
                {
                    // Om mottagaren av denna nya förfrågan redan har skickat en, acceptera den istället.
                    existingRequest.Accepted = true;
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false; // Förfrågan finns redan eller är redan vänner
            }

            var friendship = new Friendship
            {
                SenderId = senderId,
                ReceiverId = receiver.Id,
                Accepted = false
            };
            _context.Friendships.Add(friendship);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AcceptFriendRequestAsync(string currentUserId, string senderId)
        {
            var friendship = await _context.Friendships
                .FirstOrDefaultAsync(f => f.SenderId == senderId && f.ReceiverId == currentUserId && !f.Accepted);

            if (friendship == null)
            {
                return false; // Förfrågan hittades inte eller är redan accepterad
            }

            friendship.Accepted = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectFriendRequestAsync(string currentUserId, string senderId)
        {
            var friendship = await _context.Friendships
                .FirstOrDefaultAsync(f => f.SenderId == senderId && f.ReceiverId == currentUserId && !f.Accepted);

            if (friendship == null)
            {
                return false; // Förfrågan hittades inte eller är redan accepterad
            }

            _context.Friendships.Remove(friendship);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveFriendAsync(string currentUserId, string friendId)
        {
            var friendship = await _context.Friendships
                .FirstOrDefaultAsync(f =>
                    (f.SenderId == currentUserId && f.ReceiverId == friendId && f.Accepted) ||
                    (f.SenderId == friendId && f.ReceiverId == currentUserId && f.Accepted)
                );

            if (friendship == null)
            {
                return false; // Inte vänner
            }

            _context.Friendships.Remove(friendship);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<FriendDto>> GetFriendsAsync(string userId)
        {
            var friends = await _context.Friendships
                .Where(f => (f.SenderId == userId || f.ReceiverId == userId) && f.Accepted)
                .Include(f => f.Sender)
                .Include(f => f.Receiver)
                .Select(f => new FriendDto
                {
                    UserId = f.SenderId == userId ? f.ReceiverId : f.SenderId,
                    Username = f.SenderId == userId ? f.Receiver!.UserName! : f.Sender!.UserName!, // Fixed CS8601, CS8602
                    IsAccepted = f.Accepted,
                    IsSender = f.SenderId == userId // Not relevant for accepted friends, but good for consistency
                })
                .ToListAsync();

            return friends;
        }

        public async Task<List<FriendDto>> GetPendingFriendRequestsAsync(string userId)
        {
            var pendingRequests = await _context.Friendships
                .Where(f => f.ReceiverId == userId && !f.Accepted)
                .Include(f => f.Sender)
                .Select(f => new FriendDto
                {
                    UserId = f.SenderId,
                    Username = f.Sender!.UserName!, // Fixed CS8601, CS8602
                    IsAccepted = f.Accepted,
                    IsSender = true // The current user is the receiver, so the other is the sender
                })
                .ToListAsync();

            return pendingRequests;
        }
    }
}
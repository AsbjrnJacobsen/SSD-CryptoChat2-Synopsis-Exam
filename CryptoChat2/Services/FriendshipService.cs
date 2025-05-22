using CryptoChat2.Data;
using CryptoChat2.Models;
using CryptoChat2.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CryptoChat2.Services;

public class FriendshipService
{
    private readonly AppDbContext _context;

    public FriendshipService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> SendFriendRequestAsync(int requesterId, int addresseeId)
    {
        if (requesterId == addresseeId) return false;

        var exists = await _context.Friendships.AnyAsync(f =>
            (f.RequesterId == requesterId && f.AddresseeId == addresseeId) ||
            (f.RequesterId == addresseeId && f.AddresseeId == requesterId));

        if (exists) return false;

        _context.Friendships.Add(new Friendship
        {
            RequesterId = requesterId,
            AddresseeId = addresseeId,
            IsAccepted = false // Pending request
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AcceptFriendRequestAsync(int requesterId, int addresseeId)
    {
        var friendship = await _context.Friendships.FirstOrDefaultAsync(f =>
            f.RequesterId == requesterId && f.AddresseeId == addresseeId && !f.IsAccepted);

        if (friendship == null) return false;

        friendship.IsAccepted = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<FriendDto>> GetFriendsAsync(int userId)
    {
        return await _context.Friendships
            .Where(f => f.IsAccepted && (f.RequesterId == userId || f.AddresseeId == userId))
            .Select(f => f.RequesterId == userId
                ? new FriendDto
                {
                    Id = f.Addressee.Id,
                    Username = f.Addressee.Username
                }
                : new FriendDto
                {
                    Id = f.Requester.Id,
                    Username = f.Requester.Username
                })
            .ToListAsync();
    }

    public async Task<List<Friendship>> GetPendingRequestsAsync(int userId)
    {
        return await _context.Friendships
            .Where(f => f.AddresseeId == userId && !f.IsAccepted)
            .Include(f => f.Requester)
            .ToListAsync();
    }
    
    public async Task<bool> DeleteFriendAsync(int userId, int friendId)
    {
        var friendships = await _context.Friendships
            .Where(f => (f.RequesterId == userId && f.AddresseeId == friendId) ||
                        (f.RequesterId == friendId && f.AddresseeId == userId))
            .ToListAsync();

        if (!friendships.Any())
            return false;

        _context.Friendships.RemoveRange(friendships);
        await _context.SaveChangesAsync();

        return true;
    }

}

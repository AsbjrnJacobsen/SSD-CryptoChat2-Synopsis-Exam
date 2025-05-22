using CryptoChat2.Data;
using CryptoChat2.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoChat2.Services;

public class FriendChatService
{
    private readonly AppDbContext _context;

    public FriendChatService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> AreFriendsAsync(int userId, int friendId)
    {
        return await _context.Friendships.AnyAsync(f =>
            ((f.RequesterId == userId && f.AddresseeId == friendId) ||
             (f.RequesterId == friendId && f.AddresseeId == userId))
            && f.IsAccepted);
    }

    public async Task<Message?> SendFriendMessageAsync(int senderId, int receiverId, string content)
    {
        if (!await AreFriendsAsync(senderId, receiverId))
            return null;

        var message = new Message
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            Content = content,
            Timestamp = DateTime.UtcNow
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        return message;
    }

    public async Task<List<Message>> GetFriendMessagesAsync(int userId, int friendId)
    {
        if (!await AreFriendsAsync(userId, friendId))
            return new List<Message>();

        return await _context.Messages
            .Where(m => (m.SenderId == userId && m.ReceiverId == friendId) ||
                        (m.SenderId == friendId && m.ReceiverId == userId))
            .OrderBy(m => m.Timestamp)
            .ToListAsync();
    }
}
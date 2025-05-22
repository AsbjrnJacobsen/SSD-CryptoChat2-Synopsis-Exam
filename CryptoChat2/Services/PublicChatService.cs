using CryptoChat2.Data;
using CryptoChat2.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoChat2.Services;

public class PublicChatService
{
    private readonly AppDbContext _context;

    public PublicChatService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PublicMessage> SendMessageAsync(int senderId, string content)
    {
        var message = new PublicMessage
        {
            SenderId = senderId,
            Content = content,
            Timestamp = DateTime.UtcNow
        };

        _context.PublicMessages.Add(message);
        await _context.SaveChangesAsync();

        return message;
    }

    public async Task<List<PublicMessage>> GetMessagesAsync()
    {
        return await _context.PublicMessages
            .Include(m => m.Sender)
            .OrderBy(m => m.Timestamp)
            .ToListAsync();
    }
}
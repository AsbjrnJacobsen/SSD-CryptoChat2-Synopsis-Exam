using CryptoChat2.Data;
using CryptoChat2.Models;
using CryptoChat2.Models.DTOs;
using CryptoChat2.Utils;
using Microsoft.EntityFrameworkCore;

namespace CryptoChat2.Services;

public class GroupService
{
    private readonly AppDbContext _context;

    public GroupService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Group> CreateGroupAsync(string name, int ownerId)
    {
        var sanitizedName = InputSanitizer.Sanitize(name);
        var group = new Group { Name = sanitizedName, OwnerId = ownerId };

        _context.Groups.Add(group);
        _context.GroupMembers.Add(new GroupMember { GroupId = group.Id, UserId = ownerId });
        await _context.SaveChangesAsync();
        return group;
    }


    public async Task<bool> AddUserToGroupAsync(int groupId, int userId)
    {
        var group = await _context.Groups.FindAsync(groupId);
        var user = await _context.Users.FindAsync(userId);
        if (group == null || user == null) return false;

        var membershipExists = await _context.GroupMembers
            .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId);
        if (membershipExists) return false;

        _context.GroupMembers.Add(new GroupMember { GroupId = groupId, UserId = userId });
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<GroupMessage> SendGroupMessageAsync(int groupId, int senderId, string content)
    {
        var message = new GroupMessage
        {
            GroupId = groupId,
            SenderId = senderId,
            Content = content,
            Timestamp = DateTime.UtcNow
        };
        _context.GroupMessages.Add(message);
        await _context.SaveChangesAsync();
        return message;
    }

    public async Task<List<GroupMessage>> GetGroupMessagesAsync(int groupId)
    {
        return await _context.GroupMessages
            .Where(m => m.GroupId == groupId)
            .OrderBy(m => m.Timestamp)
            .ToListAsync();
    }
    
    public async Task<bool> SendEncryptedGroupMessageAsync(int groupId, int senderId, string cipherText, string nonce)
    {
        var members = await _context.GroupMembers
            .Where(gm => gm.GroupId == groupId && gm.UserId != senderId)
            .Select(gm => gm.UserId)
            .ToListAsync();

        if (!members.Any()) return false;

        var encryptedBytes = Convert.FromBase64String(cipherText);
        var nonceBytes = Convert.FromBase64String(nonce);

        var messages = members.Select(memberId => new GroupEncryptedMessage
        {
            GroupId = groupId,
            SenderId = senderId,
            ReceiverId = memberId,
            CipherText = encryptedBytes,
            Nonce = nonceBytes,
            Timestamp = DateTime.UtcNow
        });

        _context.Set<GroupEncryptedMessage>().AddRange(messages);
        await _context.SaveChangesAsync();

        return true;
    }
    
    public async Task<bool> SaveEncryptedGroupMessagesAsync(int groupId, int senderId, List<EncryptedPayloadDto> payloads)
    {
        foreach (var payload in payloads)
        {
            _context.GroupEncryptedMessages.Add(new GroupEncryptedMessage
            {
                GroupId = groupId,
                SenderId = senderId,
                ReceiverId = payload.ReceiverId,
                CipherText = Convert.FromBase64String(payload.CipherText),
                Nonce = Convert.FromBase64String(payload.Nonce),
                Timestamp = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<int>> GetGroupMemberIdsAsync(int groupId)
    {
        return await _context.GroupMembers
            .Where(gm => gm.GroupId == groupId)
            .Select(gm => gm.UserId)
            .ToListAsync();
    }

    public async Task<List<GroupEncryptedMessage>> GetEncryptedMessagesForUserAsync(int groupId, int userId)
    {
        return await _context.GroupEncryptedMessages
            .Where(m => m.GroupId == groupId &&
                        (m.ReceiverId == userId || m.SenderId == userId))
            .OrderBy(m => m.Timestamp)
            .ToListAsync();
    }


    public async Task<bool> SendGroupMessagesAsync(int groupId, int senderId, List<EncryptedPayloadDto> payloads)
    {
        var group = await _context.Groups.FindAsync(groupId);
        if (group == null) return false;

        var messages = payloads.Select(p => new GroupEncryptedMessage
        {
            GroupId = groupId,
            SenderId = senderId,
            ReceiverId = p.ReceiverId,
            CipherText = Convert.FromBase64String(p.CipherText),
            Nonce = Convert.FromBase64String(p.Nonce),
            Timestamp = DateTime.UtcNow
        }).ToList();

        _context.GroupEncryptedMessages.AddRange(messages);
        await _context.SaveChangesAsync();

        return true;
    }

    
    public async Task<bool> KickMemberAsync(int groupId, int ownerId, int userId)
    {
        var group = await _context.Groups.FindAsync(groupId);
        if (group == null || group.OwnerId != ownerId) return false;

        var membership = await _context.GroupMembers
            .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == userId);
        if (membership == null) return false;

        _context.GroupMembers.Remove(membership);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteGroupAsync(int groupId, int ownerId)
    {
        var group = await _context.Groups.FindAsync(groupId);
        if (group == null || group.OwnerId != ownerId) return false;

        _context.Groups.Remove(group);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> LeaveGroupAsync(int groupId, int userId)
    {
        var group = await _context.Groups.FindAsync(groupId);
        if (group == null) return false;

        if (group.OwnerId == userId) return false; // Owner kan ikke leave

        var membership = await _context.GroupMembers
            .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == userId);
        if (membership == null) return false;

        _context.GroupMembers.Remove(membership);
        await _context.SaveChangesAsync();
        return true;
    }


}
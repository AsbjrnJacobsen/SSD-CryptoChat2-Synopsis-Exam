using System.Security.Cryptography;
using CryptoChat2.Data;
using CryptoChat2.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoChat2.Services;

public class GroupKeyStoreDbService
{
    private readonly AppDbContext _context;

    public GroupKeyStoreDbService(AppDbContext context)
    {
        _context = context;
    }

    // Get stored public key
    public async Task<byte[]?> GetPublicKeyAsync(int groupId, int userId)
    {
        var entry = await _context.GroupKeys
            .FirstOrDefaultAsync(k => k.GroupId == groupId && k.UserId == userId);
        return entry?.PublicKey;
    }

    // Store/update public key
    public async Task StorePublicKeyAsync(int groupId, int userId, byte[] publicKey)
    {
        var existing = await _context.GroupKeys
            .FirstOrDefaultAsync(k => k.GroupId == groupId && k.UserId == userId);

        if (existing != null)
        {
            existing.PublicKey = publicKey;
        }
        else
        {
            _context.GroupKeys.Add(new GroupKey
            {
                GroupId = groupId,
                UserId = userId,
                PublicKey = publicKey
            });
        }

        await _context.SaveChangesAsync();
    }

    // Get all others' public keys
    public async Task<List<(int UserId, byte[] PublicKey)>> GetGroupKeysExceptAsync(int groupId, int excludeUserId)
    {
        return await _context.GroupKeys
            .Where(k => k.GroupId == groupId && k.UserId != excludeUserId)
            .Select(k => new ValueTuple<int, byte[]>(k.UserId, k.PublicKey))
            .ToListAsync();
    }
}
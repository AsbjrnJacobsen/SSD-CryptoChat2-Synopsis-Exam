using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CryptoChat2.Services;
using CryptoChat2.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CryptoChat2.Controllers;

[ApiController]
[Route("api/groupkeys")]
[Authorize]
public class GroupKeyExchangeController : ControllerBase
{
    private readonly GroupKeyStoreDbService _keyStore;
    private readonly AppDbContext _context;

    public GroupKeyExchangeController(GroupKeyStoreDbService keyStore, AppDbContext context)
    {
        _keyStore = keyStore;
        _context = context;
    }

    [HttpGet("{groupId}/me")]
    public async Task<IActionResult> GetOwnPublicKey(int groupId)
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        bool isMember = await _context.GroupMembers
            .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId);
        if (!isMember)
            return Forbid();

        var pubKey = await _keyStore.GetPublicKeyAsync(groupId, userId);
        if (pubKey == null)
            return NotFound("Public key not found.");

        return Ok(Convert.ToBase64String(pubKey));
    }

    [HttpGet("{groupId}/others")]
    public async Task<IActionResult> GetOtherMembersPublicKeys(int groupId)
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        bool isMember = await _context.GroupMembers
            .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId);
        if (!isMember)
            return Forbid();

        var keys = await _keyStore.GetGroupKeysExceptAsync(groupId, userId);

        var result = keys.Select(k => new
        {
            UserId = k.UserId,
            PublicKey = Convert.ToBase64String(k.PublicKey)
        });

        return Ok(result);
    }

    [HttpPost("{groupId}/upload")]
    public async Task<IActionResult> UploadPublicKey(int groupId, [FromBody] string base64PublicKey)
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        bool isMember = await _context.GroupMembers
            .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId);
        if (!isMember)
            return Forbid();

        if (string.IsNullOrWhiteSpace(base64PublicKey) || !IsValidBase64(base64PublicKey))
            return BadRequest("Invalid public key format.");

        byte[] publicKey;
        try
        {
            publicKey = Convert.FromBase64String(base64PublicKey);
        }
        catch
        {
            return BadRequest("Public key must be base64-encoded.");
        }

        await _keyStore.StorePublicKeyAsync(groupId, userId, publicKey);
        return Ok(new { message = "Public key uploaded." });
    }

    private bool IsValidBase64(string input)
    {
        Span<byte> buffer = new Span<byte>(new byte[input.Length]);
        return Convert.TryFromBase64String(input, buffer, out _);
    }
}

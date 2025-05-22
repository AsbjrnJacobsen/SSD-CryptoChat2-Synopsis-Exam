using CryptoChat2.Services;
using CryptoChat2.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CryptoChat2.Data;
//using CryptoChat2.Security;
using Microsoft.EntityFrameworkCore;

namespace CryptoChat2.Controllers;

[ApiController]
[Route("api/groups")]
[Authorize]
public class GroupController : ControllerBase
{
    private readonly GroupService _groupService;
    private readonly AppDbContext _context;
    
    public GroupController(GroupService groupService, AppDbContext context)
    {
        _groupService = groupService;
        _context = context;
    }
    
    [HttpGet("{groupId}/messages")]
    public async Task<IActionResult> GetGroupMessages(int groupId)
    {
        var messages = await _groupService.GetGroupMessagesAsync(groupId);
        return Ok(messages);
    }
    
    [HttpGet("{groupId}/encrypted-messages")]
    public async Task<IActionResult> GetMyEncryptedMessages(int groupId)
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // is user member of group
        bool isMember = await _context.GroupMembers
            .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId);

        if (!isMember)
            return Forbid(); 

        var messages = await _groupService.GetEncryptedMessagesForUserAsync(groupId, userId);

        var result = messages.Select(m => new
        {
            m.SenderId,
            CipherText = Convert.ToBase64String(m.CipherText),
            Nonce = Convert.ToBase64String(m.Nonce),
            m.Timestamp
        });

        return Ok(result);
    }
    
    [HttpGet("{groupId}/publickeys")]
    public async Task<IActionResult> GetGroupMemberPublicKeys(
        int groupId,
        [FromServices] GroupKeyStoreDbService keyStore)
    {
        var requesterId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var isMember = await _context.GroupMembers
            .AnyAsync(g => g.GroupId == groupId && g.UserId == requesterId);
        if (!isMember)
            return Forbid();

        var members = await keyStore.GetGroupKeysExceptAsync(groupId, requesterId);

        var result = members.Select(k => new
        {
            UserId = k.UserId,
            PublicKey = Convert.ToBase64String(k.PublicKey)
        });

        return Ok(result);
    }

    
    [HttpPost("createGroup")]
    public async Task<IActionResult> CreateGroup([FromBody] GroupCreateDto dto)
    {
        var ownerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var group = await _groupService.CreateGroupAsync(dto.Name, ownerId);
        return Ok(group);
    }

    [HttpPost("{groupId}/add-user-to-group")]
    public async Task<IActionResult> AddUserToGroup(int groupId, [FromBody] AddUserToGroupDto dto)
    {
        var success = await _groupService.AddUserToGroupAsync(groupId, dto.UserId);
        if (!success) return BadRequest("Group or User not found, or user already in group.");
        return Ok(new { message = "User added to group successfully." });
    }

    [HttpPost("{groupId}/send-encrypted-group-message")]
    public async Task<IActionResult> SendEncryptedGroupMessage(int groupId, [FromBody] EncryptedGroupMessageDto dto)
    {
        var senderId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var success = await _groupService.SaveEncryptedGroupMessagesAsync(groupId, senderId, dto.Payloads);
    
        if (!success) return BadRequest("Failed to save messages.");

        return Ok(new { message = "Encrypted messages saved." });
    }
    
    [HttpDelete("{groupId}/users/{userId}")]
    public async Task<IActionResult> KickMember(int groupId, int userId)
    {
        var ownerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var success = await _groupService.KickMemberAsync(groupId, ownerId, userId);
        if (!success) return Forbid();
        return Ok(new { message = "User kicked from group." });
    }

    [HttpDelete("{groupId}")]
    public async Task<IActionResult> DeleteGroup(int groupId)
    {
        var ownerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var success = await _groupService.DeleteGroupAsync(groupId, ownerId);
        if (!success) return Forbid();
        return Ok(new { message = "Group deleted successfully." });
    }

    [HttpDelete("{groupId}/leave")]
    public async Task<IActionResult> LeaveGroup(int groupId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var success = await _groupService.LeaveGroupAsync(groupId, userId);
        if (!success) return Forbid();
        return Ok(new { message = "You left the group." });
    }

}
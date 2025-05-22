using CryptoChat2.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CryptoChat2.Controllers;

[ApiController]
[Route("api/friendships")]
[Authorize]
public class FriendshipController : ControllerBase
{
    private readonly FriendshipService _friendshipService;

    public FriendshipController(FriendshipService friendshipService)
    {
        _friendshipService = friendshipService;
    }

    [HttpPost("request/{addresseeId}")]
    public async Task<IActionResult> SendFriendRequest(int addresseeId)
    {
        var requesterId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var success = await _friendshipService.SendFriendRequestAsync(requesterId, addresseeId);

        if (!success) return BadRequest("Friend request already exists or invalid.");
        return Ok(new { message = "Friend request sent." });
    }

    [HttpPost("accept/{requesterId}")]
    public async Task<IActionResult> AcceptFriendRequest(int requesterId)
    {
        var addresseeId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var success = await _friendshipService.AcceptFriendRequestAsync(requesterId, addresseeId);

        if (!success) return NotFound("Friend request not found.");
        return Ok(new { message = "Friend request accepted." });
    }

    [HttpGet("friends")]
    public async Task<IActionResult> GetFriends()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var friends = await _friendshipService.GetFriendsAsync(userId);

        return Ok(friends);
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingRequests()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var pending = await _friendshipService.GetPendingRequestsAsync(userId);

        return Ok(pending.Select(f => new
        {
            requesterId = f.RequesterId,
            requesterUsername = f.Requester.Username
        }));
    }
    
    [HttpDelete("{friendId}")]
    public async Task<IActionResult> DeleteFriend(int friendId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var success = await _friendshipService.DeleteFriendAsync(userId, friendId);
        if (!success)
            return NotFound("Friendship does not exist.");

        return Ok(new { message = "Friendship deleted successfully." });
    }

}

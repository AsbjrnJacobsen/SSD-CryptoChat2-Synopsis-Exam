using CryptoChat2.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CryptoChat2.Utils;

namespace CryptoChat2.Controllers;

[ApiController]
[Route("api/friends")]
[Authorize]
public class FriendChatController : ControllerBase
{
    private readonly FriendChatService _friendChatService;

    public FriendChatController(FriendChatService friendChatService)
    {
        _friendChatService = friendChatService;
    }

    [HttpPost("{friendId}/messages")]
    public async Task<IActionResult> SendMessage(int friendId, [FromBody] string content)
    {
        var senderId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var sanitized = InputSanitizer.Sanitize(content);

        var message = await _friendChatService.SendFriendMessageAsync(senderId, friendId, sanitized);

        if (message == null)
            return Forbid("You are not friends with this user.");

        return Ok(message);
    }


    [HttpGet("{friendId}/messages")]
    public async Task<IActionResult> GetMessages(int friendId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var messages = await _friendChatService.GetFriendMessagesAsync(userId, friendId);

        if (messages.Count == 0)
            return Forbid("You are not friends with this user or no messages found.");

        return Ok(messages);
    }
}
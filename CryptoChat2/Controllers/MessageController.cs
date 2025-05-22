using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CryptoChat2.Services;
using System.Security.Claims;
using CryptoChat2.Utils;

namespace CryptoChat2.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly MessageService _messageService;

    public MessagesController(MessageService messageService)
    {
        _messageService = messageService;
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage(int receiverId, [FromBody] string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return BadRequest("Message content cannot be empty.");

        var senderId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var sanitized = InputSanitizer.Sanitize(content);

        var message = await _messageService.SendMessageAsync(senderId, receiverId, sanitized);
        return Ok(message);
    }


    [HttpGet("{otherUserId}")]
    public async Task<IActionResult> GetMessages(int otherUserId)
    {
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var messages = await _messageService.GetMessagesBetweenUsersAsync(currentUserId, otherUserId);
        return Ok(messages);
    }
}
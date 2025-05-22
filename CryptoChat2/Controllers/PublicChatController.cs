using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CryptoChat2.Services;
using CryptoChat2.Utils;

namespace CryptoChat2.Controllers;

[ApiController]
[Route("api/publicchat")]
[Authorize]
public class PublicChatController : ControllerBase
{
    private readonly PublicChatService _chatService;

    public PublicChatController(PublicChatService chatService)
    {
        _chatService = chatService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMessages()
    {
        var messages = await _chatService.GetMessagesAsync();

        var result = messages.Select(m => new
        {
            m.Id,
            m.Content,
            m.Timestamp,
            SenderUsername = m.Sender.Username
        });

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return BadRequest("Message content cannot be empty.");

        var senderId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var sanitized = InputSanitizer.Sanitize(content); // Strip tags
        var message = await _chatService.SendMessageAsync(senderId, sanitized);

        return Ok(new
        {
            message.Id,
            message.Content,
            message.Timestamp
        });
    }

}
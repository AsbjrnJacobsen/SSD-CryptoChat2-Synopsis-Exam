using System;

namespace CryptoChat2.ClientLib.Models.DTOs;

public class GroupMessageDto
{
    public int SenderId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public int Id { get; set; }
    public int GroupId { get; set; }
}
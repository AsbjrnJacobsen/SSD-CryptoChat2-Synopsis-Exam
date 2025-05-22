using System;

namespace CryptoChat2.ClientLib.Models.DTOs;

public class MessageDto
{
    public int Id { get; set; }
    public int SenderId { get; set; }
    public int ReceiverId { get; set; } // Bruges kun til private beskeder
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
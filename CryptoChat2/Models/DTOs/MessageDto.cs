namespace CryptoChat2.Models.DTOs;

public class MessageDto
{
    public int Id { get; set; }
    public string SenderUsername { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
using System;

namespace CryptoChat2.ClientLib.Models.DTOs;

public class EncryptedGroupMessageResultDto
{
    public int SenderId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Nonce { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
using System;

namespace CryptoChat2.ClientLib.Models.DTOs;

public class EncryptedMessageDto
{
    public int SenderId { get; set; }
    public int ReceiverId { get; set; }
    public string CipherText { get; set; } = string.Empty;
    public string Nonce { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
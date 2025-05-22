namespace CryptoChat2.ClientLib.Models.DTOs;

public class EncryptedPayloadDto
{
    public int ReceiverId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Nonce { get; set; } = string.Empty;
}
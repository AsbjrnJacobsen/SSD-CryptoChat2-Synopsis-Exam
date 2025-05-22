namespace CryptoChat2.ClientLib.Models.DTOs;

public class GroupMemberPublicKeyDto
{
    public int UserId { get; set; }
    public string PublicKey { get; set; } = string.Empty; // Base64-encoded
}
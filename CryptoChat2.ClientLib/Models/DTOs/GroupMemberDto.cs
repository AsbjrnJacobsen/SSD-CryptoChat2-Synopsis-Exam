namespace CryptoChat2.ClientLib.Models.DTOs;

public class GroupMemberDto
{
    public int UserId { get; set; }
    public string PublicKey { get; set; } = string.Empty;
}
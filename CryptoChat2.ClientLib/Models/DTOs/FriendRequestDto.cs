namespace CryptoChat2.ClientLib.Models.DTOs;

public class FriendRequestDto
{
    public int RequesterId { get; set; }
    public string RequesterUsername { get; set; } = string.Empty;
}
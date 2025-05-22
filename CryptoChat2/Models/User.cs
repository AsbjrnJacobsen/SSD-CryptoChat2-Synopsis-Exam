namespace CryptoChat2.Models;

public class User
{
    public int Id { get; set; } // Primary Key
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
    public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();

    public ICollection<Friendship>? Friends { get; set; }
    public ICollection<Friendship>? FriendOf { get; set; }

    public ICollection<GroupMember> GroupMemberships { get; set; } = new List<GroupMember>();
    public ICollection<Group> OwnedGroups { get; set; } = new List<Group>();
    public ICollection<GroupMessage> SentGroupMessages { get; set; } = new List<GroupMessage>();
}
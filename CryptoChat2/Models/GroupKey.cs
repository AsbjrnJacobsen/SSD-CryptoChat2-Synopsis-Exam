namespace CryptoChat2.Models;

public class GroupKey
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int GroupId { get; set; }
    public Group Group { get; set; } = null!;

    public byte[] PublicKey { get; set; } = Array.Empty<byte>();
}
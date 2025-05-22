namespace CryptoChat2.Models;

public class GroupMessage
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public Group Group { get; set; } = null!;

    public int SenderId { get; set; }
    public User Sender { get; set; } = null!;

    public string Content { get; set; } = null!;
    public DateTime Timestamp { get; set; }
}
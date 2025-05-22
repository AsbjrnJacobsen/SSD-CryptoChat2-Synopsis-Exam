namespace CryptoChat2.Models;

public class Group
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int OwnerId { get; set; }

    public User Owner { get; set; } = null!;
    public List<GroupMember> Members { get; set; } = new();
    public List<GroupMessage> Messages { get; set; } = new();
}
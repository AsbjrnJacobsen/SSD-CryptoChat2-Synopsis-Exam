namespace CryptoChat2.Models;

public class Friendship
{
    public int Id { get; set; } // Primary Key
    public int RequesterId { get; set; }
    public int AddresseeId { get; set; }
    public bool IsAccepted { get; set; }

    public User Requester { get; set; } = null!;
    public User Addressee { get; set; } = null!;
}

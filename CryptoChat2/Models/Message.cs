using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CryptoChat2.Models;

public class Message
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int SenderId { get; set; }

    [Required]
    public int ReceiverId { get; set; }

    [Required]
    public string Content { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(SenderId))]
    public User Sender { get; set; } = null!;

    [ForeignKey(nameof(ReceiverId))]
    public User Receiver { get; set; } = null!;
}
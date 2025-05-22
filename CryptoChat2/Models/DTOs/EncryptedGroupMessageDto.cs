using System.ComponentModel.DataAnnotations;

namespace CryptoChat2.Models.DTOs;

public class EncryptedGroupMessageDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int GroupId { get; set; }

    [Required]
    public List<EncryptedPayloadDto> Payloads { get; set; } = new();
}

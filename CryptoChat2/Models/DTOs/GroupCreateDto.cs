using System.ComponentModel.DataAnnotations;

namespace CryptoChat2.Models.DTOs;

public class GroupCreateDto
{
    [Required]
    [StringLength(30, MinimumLength = 3, ErrorMessage = "Group name must be between 3 and 30 characters.")]
    public required string Name { get; set; }
}

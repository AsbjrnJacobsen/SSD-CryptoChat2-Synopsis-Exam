using System.ComponentModel.DataAnnotations;

namespace CryptoChat2.Models.DTOs;

public class AddUserToGroupDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "UserId must be a positive number.")]
    public int UserId { get; set; }
}
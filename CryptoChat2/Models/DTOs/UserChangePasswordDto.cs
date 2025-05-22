using System.ComponentModel.DataAnnotations;

namespace CryptoChat2.Models.DTOs;

public class UserChangePasswordDto
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
        ErrorMessage = "New password must contain lowercase, uppercase, and number.")]
    public string NewPassword { get; set; } = string.Empty;
}
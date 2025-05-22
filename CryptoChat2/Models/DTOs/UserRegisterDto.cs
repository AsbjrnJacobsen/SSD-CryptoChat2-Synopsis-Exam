using System.ComponentModel.DataAnnotations;

namespace CryptoChat2.Models.DTOs;

public class UserRegisterDto
{
    [Required]
    [StringLength(20, MinimumLength = 3)]
    [RegularExpression("^[a-zA-Z0-9_]+$", ErrorMessage = "Username must be alphanumeric with underscores only.")]
    public required string Username { get; set; }

    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
        ErrorMessage = "Password must contain at least one lowercase letter, one uppercase letter, and one number.")]
    public required string Password { get; set; }
}
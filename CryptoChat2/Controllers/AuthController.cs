using CryptoChat2.Models;
using System.Security.Claims;
using CryptoChat2.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CryptoChat2.Models.DTOs;
namespace CryptoChat2.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly JwtTokenService _tokenService;
    
    public AuthController(AuthService authService, JwtTokenService tokenService)
    {
        _authService = authService;
        _tokenService = tokenService;

    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegisterDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _authService.RegisterUserAsync(dto.Username, dto.Email, dto.Password);

        if (user == null)
            return BadRequest("Username is already taken.");

        return Ok(new
        {
            message = "User registered successfully",
            user.Id,
            user.Username,
            user.Email
        });
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login(UserLoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _authService.ValidateLoginAsync(dto.Username, dto.Password);

        if (user == null)
            return Unauthorized("Invalid username or password.");

        var token = _tokenService.CreateToken(user);

        return Ok(new { token, userId = user.Id });
    }

    
    [HttpGet("me")]
    [Authorize]
    public IActionResult GetMe()
    {
        var username = User.Identity?.Name;
        return Ok(new { username });
    }
    
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] UserChangePasswordDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var success = await _authService.ChangePasswordAsync(userId, dto.CurrentPassword, dto.NewPassword);

        if (!success)
            return BadRequest("Invalid current password.");

        return Ok(new { message = "Password changed successfully." });
    }


}

public record RegisterRequest(string Username, string Email, string Password);
public record LoginRequest(string Username, string Password);
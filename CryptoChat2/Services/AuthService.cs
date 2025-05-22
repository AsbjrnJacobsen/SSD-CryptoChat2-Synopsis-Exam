using CryptoChat2.Data;
using CryptoChat2.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoChat2.Services;

public class AuthService
{
    private readonly AppDbContext _context;
    private readonly PasswordHasherService _hasher;
    
    public AuthService(AppDbContext context, PasswordHasherService hasher)
    {
        _context = context;
        _hasher = hasher;
    }

    public async Task<User?> RegisterUserAsync(string username, string email, string password)
    {
        if (await _context.Users.AnyAsync(u => u.Username == username))
            return null;

        _hasher.CreatePasswordHash(password, out var hash, out var salt);

        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = hash,
            PasswordSalt = salt
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<User?> ValidateLoginAsync(string username, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null) return null;

        return _hasher.VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt)
            ? user
            : null;
    }
    
    public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        var valid = _hasher.VerifyPasswordHash(currentPassword, user.PasswordHash, user.PasswordSalt);
        if (!valid) return false;

        _hasher.CreatePasswordHash(newPassword, out var newHash, out var newSalt);
        user.PasswordHash = newHash;
        user.PasswordSalt = newSalt;

        await _context.SaveChangesAsync();
        return true;
    }

}
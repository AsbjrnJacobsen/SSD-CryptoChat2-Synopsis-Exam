using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace CryptoChat2.Services;

public class PasswordHasherService
{
    private readonly string _pepper;

    public PasswordHasherService(IConfiguration config)
    {
        _pepper = config["Security:Pepper"] ?? throw new Exception("Pepper not found in configuration.");
    }

    public void CreatePasswordHash(string password, out byte[] hash, out byte[] salt)
    {
        using var hmac = new HMACSHA512();
        salt = hmac.Key;
        var combined = Encoding.UTF8.GetBytes(password + _pepper);
        hash = hmac.ComputeHash(combined);
    }

    public bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
    {
        using var hmac = new HMACSHA512(storedSalt);
        var combined = Encoding.UTF8.GetBytes(password + _pepper);
        var computedHash = hmac.ComputeHash(combined);
        
        return computedHash.SequenceEqual(storedHash);
    }
}
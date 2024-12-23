using System.Security.Cryptography;
using System.Text;

public class PasswordService : IPasswordService
{
    public void CreatePasswordHash(string password, out string passwordHash, out string salt)
    {
        using (var hmac = new HMACSHA512())
        {
            salt = Convert.ToBase64String(hmac.Key);
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            passwordHash = Convert.ToBase64String(hash);
        }
    }

    public bool VerifyPasswordHash(string password, string storedHash, string storedSalt)
    {
        var saltBytes = Convert.FromBase64String(storedSalt);
        using (var hmac = new HMACSHA512(saltBytes))
        {
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            var computedHashString = Convert.ToBase64String(computedHash);
            return computedHashString == storedHash;
        }
    }
} 
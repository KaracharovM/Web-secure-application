public interface IPasswordService
{
    void CreatePasswordHash(string password, out string passwordHash, out string salt);
    bool VerifyPasswordHash(string password, string storedHash, string storedSalt);
} 
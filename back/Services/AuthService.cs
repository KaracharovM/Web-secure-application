using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private const int MaxLoginAttempts = 5;

    public AuthService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> RegisterUser(AuthRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
        {
            return false;
        }

        CreatePasswordHash(request.Password, out string passwordHash, out string salt);
        
        var user = new User
        {
            Username = request.Username,
            PasswordHash = passwordHash,
            Salt = salt,
            RefreshToken = "",
            RefreshTokenExpiryTime = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            IsLocked = false,
            LoginAttempts = 0
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<(bool success, User user)> LoginUser(AuthRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        
        // Если пользователь не найден
        if (user == null)
            return (false, null);

        // Если аккаунт уже заблокирован
        if (user.IsLocked)
            return (false, user);

        // Проверка пароля
        if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.Salt))
        {
            user.LoginAttempts++;
            
            // Проверяем, не превысили ли лимит попыток
            if (user.LoginAttempts >= MaxLoginAttempts)
            {
                user.IsLocked = true;
                await _context.SaveChangesAsync();
                return (false, user);
            }
            
            await _context.SaveChangesAsync();
            return (false, user);
        }

        // Успешный вход - сбрасываем счетчик
        user.LoginAttempts = 0;
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return (true, user);
    }

    private bool VerifyPasswordHash(string password, string storedHash, string storedSalt)
    {
        if (string.IsNullOrEmpty(password)) return false;
        if (string.IsNullOrEmpty(storedHash)) return false;
        if (string.IsNullOrEmpty(storedSalt)) return false;

        try
        {
            using (var hmac = new HMACSHA512(Convert.FromBase64String(storedSalt)))
            {
                var computedHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
                return computedHash == storedHash;
            }
        }
        catch
        {
            return false;
        }
    }

    private void CreatePasswordHash(string password, out string passwordHash, out string salt)
    {
        using (var hmac = new HMACSHA512())
        {
            salt = Convert.ToBase64String(hmac.Key);
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            passwordHash = Convert.ToBase64String(hash);
        }
    }
}
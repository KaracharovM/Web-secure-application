using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;
    private readonly ApplicationDbContext _context;

    public AuthController(
        IAuthService authService, 
        ITokenService tokenService,
        ApplicationDbContext context)
    {
        _authService = authService;
        _tokenService = tokenService;
        _context = context;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] AuthRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.RegisterUser(request);
        
        if (result)
            return Ok("Регистрация успешна");
        
        return BadRequest("Пользователь с таким именем уже существует");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AuthRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (success, user) = await _authService.LoginUser(request);
        
        // Проверяем существование пользователя
        if (user == null)
        {
            return BadRequest("Неверное имя пользователя или пароль");
        }

        // Проверяем блокировку
        if (user.IsLocked)
        {
            return BadRequest("Аккаунт заблокирован");
        }

        // Проверяем успешность входа
        if (!success)
        {
            if (user.LoginAttempts >= 5) // 5, потому что следующая попытка будет 6-й
            {
                return BadRequest("Аккаунт заблокирован");
            }
            return BadRequest("Неверное имя пользователя или пароль");
        }

        var token = _tokenService.GenerateJwtToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Обновляем данные пользователя
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        user.LastLoginAt = DateTime.UtcNow;
        user.LoginAttempts = 0;

        // Сохраняем изменения в базу данных
        await _context.SaveChangesAsync();

        return Ok(new AuthResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            Expiration = DateTime.UtcNow.AddMinutes(15)
        });
    }
}
 public interface IAuthService
{
    Task<bool> RegisterUser(AuthRequest request);
    Task<(bool success, User user)> LoginUser(AuthRequest request);
}
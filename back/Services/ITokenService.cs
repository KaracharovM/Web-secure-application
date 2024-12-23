public interface ITokenService
{
    string GenerateJwtToken(User user);
    string GenerateRefreshToken();
}
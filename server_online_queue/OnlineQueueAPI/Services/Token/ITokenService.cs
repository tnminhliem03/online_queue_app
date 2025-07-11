using OnlineQueueAPI.Models;

namespace OnlineQueueAPI.Services
{
    public interface ITokenService
    {
        (string accessToken, string refreshToken) GenerateToken(User user);

        string GenerateRefreshToken(User user);
    }
}
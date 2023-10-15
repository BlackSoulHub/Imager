using Imager.Domain.Models;

namespace Imager.Services.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(Guid id, string login);
    AccessTokenModel ParseAccessToken(string accessToken);
    string GenerateRefreshToken(Guid id);
    RefreshTokenModel ParseRefreshTokenAsync(string refreshToken);
}
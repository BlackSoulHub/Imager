using Imager.Domain.Errors;
using Imager.Domain.Models;
using Imager.Domain.Result;

namespace Imager.Services.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(Guid id, string login);
    Task<Result<AccessTokenModel, JwtTokenError>> ParseAccessTokenAsync(string accessToken);
    string GenerateRefreshToken(Guid id);
    Task<Result<RefreshTokenModel, JwtTokenError>> ParseRefreshTokenAsync(string refreshToken);
}
using Imager.Domain.Errors;
using Imager.Domain.Models;
using Imager.Domain.Result;

namespace Imager.Services.Interfaces;

public interface IJwtTokenService
{
    Result<string, JwtTokenError> GenerateAccessToken(Guid id, string login);
    Task<Result<AccessTokenModel, JwtTokenError>> ParseAccessTokenAsync(string accessToken);
    Result<string, JwtTokenError> GenerateRefreshToken(Guid id);
    Task<Result<RefreshTokenModel, JwtTokenError>> ParseRefreshTokenAsync(string refreshToken);
}
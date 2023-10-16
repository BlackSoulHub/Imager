using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Imager.Domain.Errors;
using Imager.Domain.Models;
using Imager.Domain.Result;
using Imager.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace Imager.Services.Implementation;

public class JwtTokenService : IJwtTokenService
{
    private readonly string _accessTokenSecret;
    private readonly string _refreshTokenSecret;

    // ReSharper disable once ConvertConstructorToMemberInitializers
    public JwtTokenService()
    {
        _accessTokenSecret = "testAccessTokenSecret";
        _refreshTokenSecret = "testRefreshTokenSecret";
    }

    public Result<string, JwtTokenError> GenerateAccessToken(Guid id, string login)
    {
        var expiredDate = DateTime.UtcNow + TimeSpan.FromHours(3);

        var claims = new List<Claim>
        {
            new("user_id", id.ToString()),
            new("user_login", login),
            new("expired_date", expiredDate.ToString(CultureInfo.InvariantCulture))
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_accessTokenSecret));

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return Result<string, JwtTokenError>.Ok(tokenHandler.WriteToken(token));
    }

    public async Task<Result<AccessTokenModel, JwtTokenError>> ParseAccessTokenAsync(string accessToken)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_accessTokenSecret));
        var tokenHandler = new JwtSecurityTokenHandler();
        
        var validationTokenResult = await tokenHandler.ValidateTokenAsync(accessToken, new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = false,
            ValidateAudience = false,
            IssuerSigningKey = key
        });

        if (!validationTokenResult.IsValid)
        {
            return Result<AccessTokenModel, JwtTokenError>.WithError(JwtTokenError.IncorrectTokenFormat);
        }

        var userId = validationTokenResult
            .Claims
            .FirstOrDefault(x => x.Key == "user_id").Value?.ToString();
        var userLogin = validationTokenResult
            .Claims
            .FirstOrDefault(x => x.Key == "user_login").Value?.ToString();
        var expiredDate = validationTokenResult
            .Claims
            .FirstOrDefault(x => x.Key == "expired_date").Value?.ToString();

        if (userId is null)
        {
            return Result<AccessTokenModel, JwtTokenError>.WithError(JwtTokenError.IncorrectTokenFormat);
        }
        
        if (userLogin is null)
        {
            return Result<AccessTokenModel, JwtTokenError>.WithError(JwtTokenError.IncorrectTokenFormat);
        }

        if (expiredDate is null)
        {
            return Result<AccessTokenModel, JwtTokenError>.WithError(JwtTokenError.IncorrectTokenFormat);
        }

        if (!Guid.TryParse(userId, out var userIdGuid))
        {
            return Result<AccessTokenModel, JwtTokenError>.WithError(JwtTokenError.IncorrectTokenFormat);
        }
        
        if (!DateTime.TryParse(expiredDate, out var expired))
        {
            return Result<AccessTokenModel, JwtTokenError>.WithError(JwtTokenError.IncorrectTokenFormat);
        }
        
        if (expired < DateTime.UtcNow)
        {
            return Result<AccessTokenModel, JwtTokenError>.WithError(JwtTokenError.TokenExpired);
        }
        
        return Result<AccessTokenModel, JwtTokenError>.Ok(new AccessTokenModel
        {
            Id = userIdGuid,
            Login = userLogin,
        });
    }

    public Result<string, JwtTokenError> GenerateRefreshToken(Guid id)
    {
        var expiredDate = DateTime.UtcNow + TimeSpan.FromDays(7);

        var claims = new List<Claim>
        {
            new("user_id", id.ToString()),
            new("expired_date", expiredDate.ToString(CultureInfo.InvariantCulture))
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_accessTokenSecret));

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return Result<string, JwtTokenError>.Ok(tokenHandler.WriteToken(token));
    }

    public async Task<Result<RefreshTokenModel, JwtTokenError>> ParseRefreshTokenAsync(string refreshToken)
    {
        throw new NotImplementedException();
    }
}
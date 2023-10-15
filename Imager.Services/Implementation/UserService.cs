using Imager.Database;
using Imager.Domain.Entities;
using Imager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Imager.Services.Implementation;

public class UserService : IUserService
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ApplicationDbContext _dbContext;
    private readonly IHashService _hashService;

    public UserService(IJwtTokenService jwtTokenService, ApplicationDbContext dbContext, IHashService hashService)
    {
        _jwtTokenService = jwtTokenService;
        _dbContext = dbContext;
        _hashService = hashService;
    }

    public async Task RegisterUserAsync(string login, string password)
    {
        var foundedUser = await GetUserByLoginAsync(login);
        if (foundedUser is not null)
        {
            throw new NotImplementedException();
        }

        var hashedPassword = _hashService.HashString(password);

        _ = await _dbContext.Users.AddAsync(new UserEntity 
        {
            Id = Guid.NewGuid(),
            Login = login,
            PasswordHash = hashedPassword,
            Friend = new List<UserEntity>()
        });
        await _dbContext.SaveChangesAsync();
    }

    public async Task<(string, string)> AuthUserAsync(string login, string password)
    {
        var foundedUser = await GetUserByLoginAsync(login);
        if (foundedUser is null)
        {
            throw new NotImplementedException();
        }
        
        var hashedPassword = _hashService.HashString(password);
        if (foundedUser.PasswordHash != hashedPassword)
        {
            throw new NotImplementedException();
        }

        var accessToken = _jwtTokenService.GenerateAccessToken(foundedUser.Id, foundedUser.Login);
        var refreshToken = _jwtTokenService.GenerateRefreshToken(foundedUser.Id);
        return (refreshToken, accessToken);
    }

    public async Task<bool> CheckIsUserFriendAsync(Guid from, Guid to)
    {
        var foundedToUser = await GetUserByIdAsync(to);
        if (foundedToUser is null)
        {
            throw new NotImplementedException();
        }

        await _dbContext.Entry(foundedToUser)
            .Collection(u => u.Friend)
            .LoadAsync();

        return foundedToUser.Friend.Any(u => u.Id == from);
    }

    public async Task<UserEntity?> GetUserByIdAsync(Guid id)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<UserEntity?> GetUserByLoginAsync(string login)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(u => u.Login == login);
    }
}
using Imager.Database;
using Imager.Domain.Entities;
using Imager.Domain.Errors;
using Imager.Domain.Result;
using Imager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Imager.Services.Implementation;

public class UserService : IUserService
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ApplicationDbContext _dbContext;
    private readonly IHashService _hashService;
    private readonly ILogger<UserService> _logger;

    public UserService(IJwtTokenService jwtTokenService, ApplicationDbContext dbContext, IHashService hashService, 
        ILogger<UserService> logger)
    {
        _jwtTokenService = jwtTokenService;
        _dbContext = dbContext;
        _hashService = hashService;
        _logger = logger;
    }

    public async Task<EmptyResult<UserError>> RegisterUserAsync(string login, string password)
    {
        var foundedUser = await GetUserByLoginAsync(login);
        if (foundedUser is not null)
        {
            return EmptyResult<UserError>.WithError(UserError.LoginAlreadyTaken);
        }

        var hashedPassword = _hashService.HashString(password);

        var newUserId = Guid.NewGuid();
        
        _ = await _dbContext.Users.AddAsync(new UserEntity 
        {
            Id = newUserId,
            Login = login,
            PasswordHash = hashedPassword,
            Friends = new FriendListEntity
            {
                Id = Guid.NewGuid(),
                UserId = newUserId,
                List = new List<UserEntity>()
            }
        });
        await _dbContext.SaveChangesAsync();
        return EmptyResult<UserError>.Ok();
    }

    public async Task<Result<(string, string), UserError>> AuthUserAsync(string login, string password)
    {
        var foundedUser = await GetUserByLoginAsync(login);
        if (foundedUser is null)
        {
            _logger.LogInformation("User with login = {login} not found", login);
            return Result<(string, string), UserError>.WithError(UserError.InvalidAuthData);
        }
        
        var hashedPassword = _hashService.HashString(password);
        if (foundedUser.PasswordHash != hashedPassword)
        {
            _logger.LogInformation("User with login = {login} and password ### not found", login);
            return Result<(string, string), UserError>.WithError(UserError.InvalidAuthData);
        }

        var accessToken = _jwtTokenService.GenerateAccessToken(foundedUser.Id, foundedUser.Login);
        var refreshToken = _jwtTokenService.GenerateRefreshToken(foundedUser.Id);
        
        return Result<(string, string), UserError>.Ok((accessToken, refreshToken));
    }

    public async Task<Result<bool, UserError>> CheckIsUserFriendAsync(Guid from, Guid to)
    {
        if (from == to)
        {
            return Result<bool, UserError>.Ok(true);
        }
        
        var foundedToUser = await GetUserByIdAsync(to);
        if (foundedToUser is null)
        {
            return Result<bool, UserError>.WithError(UserError.UserNotFound);
        }

        await _dbContext.Entry(foundedToUser)
            .Reference(u => u.Friends)
            .LoadAsync();

        await _dbContext.Entry(foundedToUser.Friends)
            .Collection(f => f.List)
            .LoadAsync();

        var userIsFriend = foundedToUser.Friends.List.Any(f => f.Id == from);

        return Result<bool, UserError>.Ok(userIsFriend);
    }

    public async Task<UserEntity?> GetUserByIdAsync(Guid id)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<UserEntity?> GetUserByLoginAsync(string login)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(u => u.Login == login);
    }

    public async Task<EmptyResult<UserError>> AddFriendAsync(Guid from, Guid to)
    {
        var foundedFromUser = await GetUserByIdAsync(from);
        if (foundedFromUser is null)
        {
            _logger.LogWarning("Пользователь с Id = {userId} не найден", from);
            return EmptyResult<UserError>.WithError(UserError.UserNotFound);
        }
        
        var foundedToUser = await GetUserByIdAsync(to);
        if (foundedToUser is null)
        {
            _logger.LogWarning("Пользователь для добавления в друзья с Id = {userId} не найден", to);
            return EmptyResult<UserError>.WithError(UserError.UserNotFound);
        }

        var userEntry = _dbContext.Entry(foundedFromUser);
        await userEntry.Reference(u => u.Friends)
            .LoadAsync();
        
        var friendEntry = _dbContext.Entry(userEntry.Entity.Friends);
        await friendEntry.Collection(f => f.List)
            .LoadAsync();

        var friendsList = friendEntry.Entity;
        if (friendsList.List.Any(u => u.Id == to))
        {
            _logger.LogInformation("Пользователь с Id = {toId} уже в списке друзей пользователя с Id = {fromId}", 
                to, from);
            return EmptyResult<UserError>.Ok();
        }

        friendsList.List.Add(foundedToUser);
        _dbContext.Update(friendsList);
        await _dbContext.SaveChangesAsync();
        return EmptyResult<UserError>.Ok();
    }
}
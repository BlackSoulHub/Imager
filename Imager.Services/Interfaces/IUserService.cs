using Imager.Domain.DTO.UserEntity;
using Imager.Domain.Entities;

namespace Imager.Services.Interfaces;

public interface IUserService
{
    Task RegisterUserAsync(string login, string password);
    Task<(string, string)> AuthUserAsync(string login, string password);
    /// <summary>
    /// Проверяет является ли пользователь с Id = from другом пользователю с Id = to
    /// </summary>
    /// <param name="from">Id пользователя для поиска в списке друзей пользователя с Id = <paramref name="to"/></param>.
    /// <param name="to">Id пользователя для поиска</param>
    /// <returns>True - если является, иначе - false</returns>
    Task<bool> CheckIsUserFriendAsync(Guid from, Guid to);

    Task<UserEntity?> GetUserByIdAsync(Guid id);
    Task<UserEntity?> GetUserByLoginAsync(string login);
}
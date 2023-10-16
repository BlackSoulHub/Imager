using Imager.Domain.DTO.UserEntity;
using Imager.Domain.Entities;
using Imager.Domain.Errors;
using Imager.Domain.Result;

namespace Imager.Services.Interfaces;

public interface IUserService
{
    Task<EmptyResult<UserError>> RegisterUserAsync(string login, string password);
    Task<Result<(string, string), UserError>> AuthUserAsync(string login, string password);
    /// <summary>
    /// Проверяет является ли пользователь с Id = from другом пользователю с Id = to.
    /// </summary>
    /// <param name="from">Id пользователя для поиска в списке друзей пользователя с Id = <paramref name="to"/></param>.
    /// <param name="to">Id пользователя для поиска.</param>
    /// <returns>True - если является, иначе - false.</returns>
    Task<Result<bool, UserError>> CheckIsUserFriendAsync(Guid from, Guid to);

    Task<UserEntity?> GetUserByIdAsync(Guid id);
    Task<UserEntity?> GetUserByLoginAsync(string login);
    /// <summary>
    /// Метод добавляет пользователя to в список друзей пользователя from.
    /// </summary>
    /// <param name="from">В чей список добавить.</param>
    /// <param name="to">Кого добавить.</param>
    Task<EmptyResult<UserError>> AddFriendAsync(Guid from, Guid to);
}
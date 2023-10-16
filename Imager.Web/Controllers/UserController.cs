using Imager.Domain.Errors;
using Imager.Services.Interfaces;
using Imager.Web.Filters;
using Imager.Web.Models.Requests;
using Imager.Web.Models.Requests.User;
using Imager.Web.Models.Responses.User;
using Microsoft.AspNetCore.Mvc;

namespace Imager.Web.Controllers;

[Route("api/user")]
public class UserController : BaseController
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;
    private readonly IImageService _imageService;
    
    public UserController(IUserService userService, ILogger<UserController> logger, IImageService imageService)
    {
        _userService = userService;
        _logger = logger;
        _imageService = imageService;
    }

    [HttpPost("registration")]
    public async Task<IActionResult> Registration(RegistrationRequest data)
    {
        var registrationResult = await _userService.RegisterUserAsync(data.Login, data.Password);
        if (registrationResult.IsFailure)
        {
            var error = registrationResult.UnwrapError();
            if (error is UserError.LoginAlreadyTaken)
            {
                return BadRequest(new BaseErrorResponse
                {
                    Message = $"Логин '{data.Login}' уже занят"
                });
            }
            
            _logger.LogError("Неизвестная ошибка регистрации: {error}", error);
            return ServerError();
        }

        return Ok();
    }

    [HttpPost("authorization")]
    public async Task<IActionResult> Authorization(AuthorizationRequest data)
    {
        var authorizationResult = await _userService.AuthUserAsync(data.Login, data.Password);
        if (authorizationResult.IsFailure)
        {
            var error = authorizationResult.UnwrapError();
            if (error is UserError.InvalidAuthData)
            {
                return Error("Неверное имя пользователя и пароль", 400);
            }

            _logger.LogError("Неожиданная ошибка авторизации: {error}", error);
            return ServerError();
        }

        var (accessToken, refreshToken) = authorizationResult.Unwrap();
        return Ok(new AuthorizationResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        });
    }

    [Authorization]
    [HttpGet("me/images")]
    public async Task<IActionResult> GetSelfImages()
    {
        var userData = await GetUserDataAsync();

        var userImages = await _imageService.GetUserImagesAsync(userData.Id);
        var imageEntityDtos = userImages.ToList();
        return Ok(new GetUserImageListResponse
        {
            Data = imageEntityDtos,
            Count = imageEntityDtos.Count
        });
    }

    [Authorization]
    [HttpGet("{userId}/images")]
    public async Task<IActionResult> GetUserImages(Guid userId)
    {
        var requestUserData = await GetUserDataAsync();

        var hasAccessResult = await _userService.CheckIsUserFriendAsync(requestUserData.Id, userId);
        if (hasAccessResult.IsFailure)
        {
            var error = hasAccessResult.UnwrapError();
            if (error is UserError.UserNotFound)
            {
                _logger.LogInformation("Пользователь с Id = {userId} не найден", userId);
                return NotFound();
            }

            _logger.LogError("Неожиданная ошибка: {error}", error);
            return ServerError();
        }

        var hasAccess = hasAccessResult.Unwrap();
        if (!hasAccess)
        {
            return NotAccess();
        }
        
        var userImages = await _imageService.GetUserImagesAsync(userId);
        var imageEntityDtos = userImages.ToList();
        return Ok(new GetUserImageListResponse
        {
            Data = imageEntityDtos,
            Count = imageEntityDtos.Count
        });
    }

    [Authorization]
    [HttpPost("me/friends/{userId}")]
    public async Task<IActionResult> AddFriend(Guid userId)
    {
        var requestUserData = await GetUserDataAsync();

        var addFriendResult = await _userService.AddFriendAsync(requestUserData.Id, userId);
        if (addFriendResult.IsFailure)
        {
            var error = addFriendResult.UnwrapError();
            if (error is UserError.UserNotFound)
            {
                return Error("Пользователь не найден", 400);
            }
            
            _logger.LogError("Неожиданная ошибка при добавлении пользователя в друзья: {error}", error);
            return ServerError();
        }

        return Ok();
    }
}
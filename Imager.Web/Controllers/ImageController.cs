using Imager.Domain.Errors;
using Imager.Services.Interfaces;
using Imager.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Imager.Web.Controllers;

[Route("api/image")]
public class ImageController : BaseController
{
    private readonly IImageService _imageService;
    private readonly ILogger<ImageController> _logger;
    private readonly IUserService _userService;
    private readonly string _savePath;
    
    public ImageController(IConfiguration configuration, IImageService imageService, ILogger<ImageController> logger, 
        IUserService userService)
    {
        _imageService = imageService;
        _logger = logger;
        _userService = userService;
        _savePath = configuration.GetRequiredSection("uploadDir")
            .Value ?? throw new NullReferenceException("Upload dir is null");
    }

    
    [Authorization]
    [HttpPost("upload")]
    public async Task<IActionResult> UploadImage(IFormFile image)
    {
        var userData = await GetUserDataAsync();

        var uploadImageResult = await _imageService.UploadImageAsync(image, userData.Id);
        if (uploadImageResult.IsFailure)
        {
            var error = uploadImageResult.UnwrapError();
            if (error is ImageError.AuthorNotFound)
            {
                _logger.LogWarning(
                    "Пользователь с Id = {userId} попытался загрузить изображение, но не был найден",
                    userData.Id);
                return Error("Произошла ошибка. Попробуйте авторизоваться снова", 400);
            }

            _logger.LogError("Произошла неожиданная ошибка при загрузке изображения: {error}", error);
            return ServerError();
        }

        return Ok();
    }

    [Authorization]
    [HttpGet("{imageId}")]
    public async Task<IActionResult> GetImage(Guid imageId)
    {
        var foundedImage = await _imageService.GetImageByIdAsync(imageId);
        if (foundedImage is null)
        {
            return Error("Изображение с запрашиваемым Id не найдено", 404);
        }

        var userData = await GetUserDataAsync();

        var userHasAccessResult = await _userService.CheckIsUserFriendAsync(userData.Id, foundedImage.Author.Id);
        if (userHasAccessResult.IsFailure)
        {
            _logger.LogError("Произошла неожиданная ошибка при проверки друзей пользователя с Id = {userId}", 
                foundedImage.Author.Id);
            return ServerError();
        }

        if (!userHasAccessResult.Unwrap())
        {
            return NotAccess();
        }

        var filePath = Path.Combine(_savePath, foundedImage.FileSystemName);
        return new FileStreamResult(new FileStream(filePath, FileMode.Open), "image/any");
    }
}
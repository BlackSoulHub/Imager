using Imager.Domain.DTO.ImageEntity;
using Imager.Domain.Entities;
using Imager.Domain.Errors;
using Imager.Domain.Result;
using Microsoft.AspNetCore.Http;

namespace Imager.Services.Interfaces;

public interface IImageService
{
    Task<IEnumerable<ImageEntityDto>> GetUserImagesAsync(Guid userId);
    Task<Result<ImageEntity, ImageError>> UploadImageAsync(IFormFile file, Guid authorId);
    Task<ImageEntity?> GetImageByIdAsync(Guid imageId);
}
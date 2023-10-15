using Imager.Domain.DTO.ImageEntity;
using Imager.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace Imager.Services.Interfaces;

public interface IImageService
{
    Task<IEnumerable<ImageEntityDto>> GetUserImagesAsync(Guid userId);
    Task<ImageEntity> UploadImageAsync(IFormFile file, Guid authorId);
}
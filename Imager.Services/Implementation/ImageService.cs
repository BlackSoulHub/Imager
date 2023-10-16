using Imager.Database;
using Imager.Domain.DTO.ImageEntity;
using Imager.Domain.Entities;
using Imager.Domain.Errors;
using Imager.Domain.Result;
using Imager.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Imager.Services.Implementation;

public class ImageService : IImageService
{
    private readonly string _uploadDir;
    private readonly ApplicationDbContext _dbContext;
    private readonly IUserService _userService;

    public ImageService(IConfiguration configuration, ApplicationDbContext dbContext, IUserService userService)
    {
        _uploadDir = configuration.GetRequiredSection("uploadDir").Value 
                     ?? throw new NullReferenceException("Upload dir is null");
        _dbContext = dbContext;
        _userService = userService;
        
        if (!Directory.Exists(_uploadDir))
        {
            Directory.CreateDirectory(_uploadDir);
        }
    }

    public async Task<IEnumerable<ImageEntityDto>> GetUserImagesAsync(Guid userId)
    {
        return await _dbContext.Images.Where(i => i.Author.Id == userId)
            .Select(i => new ImageEntityDto
            {
                Id = i.Id,
                UploadedAt = i.UploadedAt
            })
            .ToListAsync();
    }

    public async Task<Result<ImageEntity, ImageError>> UploadImageAsync(IFormFile file, Guid authorId)
    {
        var author = await _userService.GetUserByIdAsync(authorId);
        if (author is null)
        {
            return Result<ImageEntity, ImageError>.WithError(ImageError.AuthorNotFound);
        }
        
        var newFileName = Guid.NewGuid().ToString();
        var filePath = Path.Combine(_uploadDir, newFileName);
        
        using (var fileStream = File.Create(filePath))
        {
            await file.CopyToAsync(fileStream);
        }

        var newImageEntity = new ImageEntity
        {
            Id = Guid.NewGuid(),
            FileSystemName = newFileName,
            UploadedAt = DateTime.UtcNow,
            Author = author
        };
        var entry = await _dbContext.AddAsync(newImageEntity);
        await _dbContext.SaveChangesAsync();
        
        return Result<ImageEntity, ImageError>.Ok(entry.Entity);
    }

    public async Task<ImageEntity?> GetImageByIdAsync(Guid imageId)
    {
        return await _dbContext.Images
            .Include(i => i.Author)
            .FirstOrDefaultAsync(i => i.Id == imageId);
    }
}
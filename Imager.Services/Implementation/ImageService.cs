using Imager.Database;
using Imager.Domain.DTO.ImageEntity;
using Imager.Domain.Entities;
using Imager.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Imager.Services.Implementation;

public class ImageService : IImageService
{
    private readonly string _uploadDir;
    private readonly ApplicationDbContext _dbContext;
    private readonly IUserService _userService;

    public ImageService(string uploadDir, ApplicationDbContext dbContext, IUserService userService)
    {
        _uploadDir = uploadDir;
        _dbContext = dbContext;
        _userService = userService;
    }

    public async Task<IEnumerable<ImageEntityDto>> GetUserImagesAsync(Guid userId)
    {
        return await _dbContext.Images.Where(i => i.Author.Id.Equals(userId))
            .Select(i => new ImageEntityDto
            {
                Id = i.Id,
                Link = i.FileSystemName,
                UploadedAt = i.UploadedAt
            })
            .ToListAsync();
    }

    public async Task<ImageEntity> UploadImageAsync(IFormFile file, Guid authorId)
    {
        var author = await _userService.GetUserByIdAsync(authorId);
        if (author is null)
        {
            throw new NotImplementedException();
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
        return entry.Entity;
    }
}
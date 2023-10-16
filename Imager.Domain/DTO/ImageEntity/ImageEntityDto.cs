namespace Imager.Domain.DTO.ImageEntity;

public class ImageEntityDto
{
    public required Guid Id { get; init; }
    public required DateTime UploadedAt { get; init; }
}
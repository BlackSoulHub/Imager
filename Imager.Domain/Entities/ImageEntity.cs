namespace Imager.Domain.Entities;

public class ImageEntity
{
    public required Guid Id { get; init; }
    public required string FileSystemName { get; set; }
    public required DateTime UploadedAt { get; set; }
    public required UserEntity Author { get; set; }
}
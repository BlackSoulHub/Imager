namespace Imager.Domain.Entities;

public class UserEntity
{
    public required Guid Id { get; init; }
    public required string Login { get; set; }
    public required string PasswordHash { get; set; }
    public required FriendListEntity Friends { get; set; }
    public IReadOnlyCollection<ImageEntity> Images => _images;

    private List<ImageEntity> _images;
}
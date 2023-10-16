namespace Imager.Domain.Entities;

public class FriendListEntity
{
    public required Guid Id { get; init; }
    public UserEntity User { get; init; } = null!;
    public required Guid UserId { get; init; }
    public required List<UserEntity> List { get; set; }
}
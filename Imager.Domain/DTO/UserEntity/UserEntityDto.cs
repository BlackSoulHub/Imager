using Imager.Domain.DTO.ImageEntity;

namespace Imager.Domain.DTO.UserEntity;

public class UserEntityDto
{
    public required Guid Id { get; init; }
    public required string Login { get; init; }
    public required IEnumerable<UserEntityDto> Friends { get; init; }
    public IReadOnlyCollection<ImageEntityDto>? Images { get; init; }
}
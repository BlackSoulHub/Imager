namespace Imager.Domain.Models;

public class AccessTokenModel
{
    public required Guid Id { get; set; }
    public required string Login { get; set; }
}
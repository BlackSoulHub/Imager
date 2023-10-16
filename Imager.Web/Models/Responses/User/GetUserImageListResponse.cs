using System.Text.Json.Serialization;
using Imager.Domain.DTO.ImageEntity;

namespace Imager.Web.Models.Responses.User;

public class GetUserImageListResponse
{
    [JsonPropertyName("data")]
    public required IEnumerable<ImageEntityDto> Data { get; init; }
    [JsonPropertyName("count")]
    public required int Count { get; init; }
}
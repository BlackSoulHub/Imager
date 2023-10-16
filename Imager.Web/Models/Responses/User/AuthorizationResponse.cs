using System.Text.Json.Serialization;

namespace Imager.Web.Models.Responses.User;

public class AuthorizationResponse
{
    [JsonPropertyName("accessToken")]
    public required string AccessToken { get; init; }
    [JsonPropertyName("refreshToken")]
    public required string RefreshToken { get; init; }
}
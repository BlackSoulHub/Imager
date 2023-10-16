using System.Text.Json.Serialization;

namespace Imager.Web.Models.Requests;

public class BaseErrorResponse
{
    [JsonPropertyName("message")]
    public required string Message { get; init; }
}
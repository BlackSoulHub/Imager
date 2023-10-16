﻿using System.Text.Json.Serialization;

namespace Imager.Web.Models.Requests.User;

public class RegistrationRequest
{
    [JsonPropertyName("login")]
    public required string Login { get; init; }
    [JsonPropertyName("password")]
    public required string Password { get; init; }
}
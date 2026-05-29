using System.Text.Json.Serialization;

namespace SocialNetwork.Models;

public class UserResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("second_name")]
    public string SecondName { get; set; } = string.Empty;

    [JsonPropertyName("birthdate")]
    public string Birthdate { get; set; } = string.Empty;

    [JsonPropertyName("gender")]
    public string Gender { get; set; } = string.Empty;

    [JsonPropertyName("biography")]
    public string Biography { get; set; } = string.Empty;

    [JsonPropertyName("city")]
    public string City { get; set; } = string.Empty;
}

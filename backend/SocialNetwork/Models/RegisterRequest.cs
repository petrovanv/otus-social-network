using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SocialNetwork.Models;

public class RegisterRequest
{
    [Required]
    [EmailAddress]
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("second_name")]
    public string SecondName { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("birthdate")]
    public string Birthdate { get; set; } = string.Empty;

    [JsonPropertyName("gender")]
    public string Gender { get; set; } = string.Empty;

    [JsonPropertyName("biography")]
    public string Biography { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("city")]
    public string City { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
}

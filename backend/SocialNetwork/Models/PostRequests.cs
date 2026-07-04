using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SocialNetwork.Models;

public class CreatePostRequest
{
    [Required]
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}

public class UpdatePostRequest
{
    [Required]
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}

public class DeletePostRequest
{
    [Required]
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
}

using System.Text.Json.Serialization;

namespace SocialNetwork.Models;

public class Post
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("author_user_id")]
    public string AuthorUserId { get; set; } = string.Empty;

    // В кэше используется для сортировки; в API-ответ не отдаётся
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
}

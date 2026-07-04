using SocialNetwork.Models;

namespace SocialNetwork.Repositories;

public interface IPostRepository
{
    Task<Post> CreateAsync(string authorId, string text);
    Task<bool> UpdateAsync(string postId, string authorId, string text);
    Task<Post?> DeleteAsync(string postId, string authorId);
    Task<Post?> GetByIdAsync(string postId);

    /// <summary>Последние N постов указанных авторов по времени — для холодной пересборки ленты</summary>
    Task<List<Post>> GetFeedFromDbAsync(List<string> friendIds, int limit);
}

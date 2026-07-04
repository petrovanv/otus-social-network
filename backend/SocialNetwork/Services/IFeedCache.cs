using SocialNetwork.Models;

namespace SocialNetwork.Services;

public interface IFeedCache
{
    /// <summary>Прочитать ленту из кэша. Если кэша нет — построить через rebuild() и закэшировать.</summary>
    Task<List<Post>> GetFeedAsync(string userId, int offset, int limit, Func<Task<List<Post>>> rebuild);

    /// <summary>Fan-out: добавить пост в ленты подписчиков (только если их лента уже материализована).</summary>
    Task PushToFollowersAsync(Post post, IEnumerable<string> followerIds);

    /// <summary>Сбросить кэш ленты пользователя (при update/delete поста, смене состава друзей).</summary>
    Task InvalidateAsync(IEnumerable<string> userIds);
}

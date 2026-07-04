namespace SocialNetwork.Repositories;

public interface IFriendRepository
{
    Task AddAsync(string userId, string friendId);
    Task DeleteAsync(string userId, string friendId);

    /// <summary>ID друзей пользователя (чьи посты он видит в ленте)</summary>
    Task<List<string>> GetFriendIdsAsync(string userId);

    /// <summary>ID подписчиков автора (кому доставлять пост при fan-out)</summary>
    Task<List<string>> GetFollowerIdsAsync(string authorId);
}

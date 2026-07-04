using StackExchange.Redis;
using SocialNetwork.Models;
using System.Text.Json;

namespace SocialNetwork.Services;

/// <summary>
/// Кэш ленты друзей на Redis. Модель fan-out on write:
///  - при создании поста он раскладывается (LPUSHX) в ленты подписчиков;
///  - каждая лента — Redis LIST feed:{userId}, обрезается до 1000 последних (LTRIM);
///  - чтение ленты — это LRANGE (без JOIN'ов по БД);
///  - при холодном кэше лента лениво пересобирается из БД и материализуется.
/// </summary>
public class RedisFeedCache : IFeedCache
{
    private const int MaxFeedSize = 1000;                      // держим последние 1000 обновлений
    private static readonly TimeSpan Ttl = TimeSpan.FromHours(1);

    private readonly IDatabase _db;

    public RedisFeedCache(IConnectionMultiplexer redis) => _db = redis.GetDatabase();

    private static string Key(string userId) => $"feed:{userId}";

    public async Task<List<Post>> GetFeedAsync(string userId, int offset, int limit, Func<Task<List<Post>>> rebuild)
    {
        var key = Key(userId);

        if (await _db.KeyExistsAsync(key))
        {
            // Cache hit — просто срез списка
            var slice = await _db.ListRangeAsync(key, offset, offset + limit - 1);
            return slice.Select(v => Deserialize(v!)).Where(p => p != null).Select(p => p!).ToList();
        }

        // Cache miss — собираем ленту из БД и материализуем в Redis (newest-first)
        var posts = await rebuild();
        if (posts.Count > 0)
        {
            var values = posts.Take(MaxFeedSize).Select(p => (RedisValue)Serialize(p)).ToArray();
            var tran = _db.CreateTransaction();
            _ = tran.KeyDeleteAsync(key);
            _ = tran.ListRightPushAsync(key, values);          // RPUSH newest-first -> index 0 = самый свежий
            _ = tran.KeyExpireAsync(key, Ttl);
            await tran.ExecuteAsync();
        }

        return posts.Skip(offset).Take(limit).ToList();
    }

    public async Task PushToFollowersAsync(Post post, IEnumerable<string> followerIds)
    {
        var value = (RedisValue)Serialize(post);
        var batch = _db.CreateBatch();
        var tasks = new List<Task>();

        foreach (var followerId in followerIds)
        {
            var key = Key(followerId);
            // LPUSHX: кладём только если лента уже материализована.
            // Если её нет — не создаём частичную; она соберётся из БД при первом чтении.
            tasks.Add(batch.ListLeftPushAsync(key, value, when: When.Exists));
            tasks.Add(batch.ListTrimAsync(key, 0, MaxFeedSize - 1));
            tasks.Add(batch.KeyExpireAsync(key, Ttl));
        }

        batch.Execute();
        await Task.WhenAll(tasks);
    }

    public async Task InvalidateAsync(IEnumerable<string> userIds)
    {
        var keys = userIds.Select(id => (RedisKey)Key(id)).ToArray();
        if (keys.Length > 0)
            await _db.KeyDeleteAsync(keys);
    }

    private static string Serialize(Post p) => JsonSerializer.Serialize(p);
    private static Post? Deserialize(string json) => JsonSerializer.Deserialize<Post>(json);
}

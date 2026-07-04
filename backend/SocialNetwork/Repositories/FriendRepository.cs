using MySqlConnector;
using SocialNetwork.Services;

namespace SocialNetwork.Repositories;

public class FriendRepository : IFriendRepository
{
    private readonly IConnectionRouter _router;

    public FriendRepository(IConnectionRouter router) => _router = router;

    public async Task AddAsync(string userId, string friendId)
    {
        await using var conn = new MySqlConnection(_router.GetWriteConnectionString());
        await conn.OpenAsync();
        const string sql = "INSERT IGNORE INTO friends (user_id, friend_id) VALUES (@u, @f)";
        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@u", userId);
        cmd.Parameters.AddWithValue("@f", friendId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(string userId, string friendId)
    {
        await using var conn = new MySqlConnection(_router.GetWriteConnectionString());
        await conn.OpenAsync();
        const string sql = "DELETE FROM friends WHERE user_id = @u AND friend_id = @f";
        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@u", userId);
        cmd.Parameters.AddWithValue("@f", friendId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<List<string>> GetFriendIdsAsync(string userId)
    {
        await using var conn = new MySqlConnection(_router.GetReadConnectionString());
        await conn.OpenAsync();
        const string sql = "SELECT friend_id FROM friends WHERE user_id = @u";
        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@u", userId);
        return await ReadIdsAsync(cmd);
    }

    public async Task<List<string>> GetFollowerIdsAsync(string authorId)
    {
        await using var conn = new MySqlConnection(_router.GetReadConnectionString());
        await conn.OpenAsync();
        const string sql = "SELECT user_id FROM friends WHERE friend_id = @a";
        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@a", authorId);
        return await ReadIdsAsync(cmd);
    }

    private static async Task<List<string>> ReadIdsAsync(MySqlCommand cmd)
    {
        var ids = new List<string>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            ids.Add(reader.GetString(0));
        return ids;
    }
}

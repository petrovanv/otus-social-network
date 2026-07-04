using MySqlConnector;
using SocialNetwork.Models;
using SocialNetwork.Services;

namespace SocialNetwork.Repositories;

public class PostRepository : IPostRepository
{
    private readonly IConnectionRouter _router;

    public PostRepository(IConnectionRouter router) => _router = router;

    public async Task<Post> CreateAsync(string authorId, string text)
    {
        var post = new Post
        {
            Id = Guid.NewGuid().ToString(),
            AuthorUserId = authorId,
            Text = text,
            CreatedAt = DateTime.UtcNow
        };

        await using var conn = new MySqlConnection(_router.GetWriteConnectionString());
        await conn.OpenAsync();
        const string sql = "INSERT INTO posts (id, author_user_id, text, created_at) VALUES (@id, @a, @t, @c)";
        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", post.Id);
        cmd.Parameters.AddWithValue("@a", post.AuthorUserId);
        cmd.Parameters.AddWithValue("@t", post.Text);
        cmd.Parameters.AddWithValue("@c", post.CreatedAt);
        await cmd.ExecuteNonQueryAsync();
        return post;
    }

    public async Task<bool> UpdateAsync(string postId, string authorId, string text)
    {
        await using var conn = new MySqlConnection(_router.GetWriteConnectionString());
        await conn.OpenAsync();
        // authorId в WHERE — редактировать можно только свой пост
        const string sql = "UPDATE posts SET text = @t WHERE id = @id AND author_user_id = @a";
        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@t", text);
        cmd.Parameters.AddWithValue("@id", postId);
        cmd.Parameters.AddWithValue("@a", authorId);
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<Post?> DeleteAsync(string postId, string authorId)
    {
        var post = await GetByIdAsync(postId);
        if (post == null || post.AuthorUserId != authorId) return null;

        await using var conn = new MySqlConnection(_router.GetWriteConnectionString());
        await conn.OpenAsync();
        const string sql = "DELETE FROM posts WHERE id = @id AND author_user_id = @a";
        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", postId);
        cmd.Parameters.AddWithValue("@a", authorId);
        var affected = await cmd.ExecuteNonQueryAsync();
        return affected > 0 ? post : null;
    }

    public async Task<Post?> GetByIdAsync(string postId)
    {
        await using var conn = new MySqlConnection(_router.GetReadConnectionString());
        await conn.OpenAsync();
        const string sql = "SELECT id, author_user_id, text, created_at FROM posts WHERE id = @id";
        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", postId);
        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;
        return Map(reader);
    }

    public async Task<List<Post>> GetFeedFromDbAsync(List<string> friendIds, int limit)
    {
        var posts = new List<Post>();
        if (friendIds.Count == 0) return posts;

        await using var conn = new MySqlConnection(_router.GetReadConnectionString());
        await conn.OpenAsync();

        // IN (...) с параметрами — защита от инъекций
        var names = friendIds.Select((_, i) => $"@f{i}").ToArray();
        var sql = $@"SELECT id, author_user_id, text, created_at FROM posts
                     WHERE author_user_id IN ({string.Join(",", names)})
                     ORDER BY created_at DESC LIMIT {limit}";
        await using var cmd = new MySqlCommand(sql, conn);
        for (var i = 0; i < friendIds.Count; i++)
            cmd.Parameters.AddWithValue(names[i], friendIds[i]);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            posts.Add(Map(reader));
        return posts;
    }

    private static Post Map(MySqlDataReader r) => new()
    {
        Id = r.GetString("id"),
        AuthorUserId = r.GetString("author_user_id"),
        Text = r.GetString("text"),
        CreatedAt = r.GetDateTime("created_at")
    };
}

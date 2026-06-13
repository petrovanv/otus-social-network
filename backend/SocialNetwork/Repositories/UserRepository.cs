using MySqlConnector;
using SocialNetwork.Models;

namespace SocialNetwork.Repositories;

public class UserRepository : IUserRepository
{
    private readonly string _connectionString;

    public UserRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("MySql")
            ?? throw new InvalidOperationException("MySql connection string not configured");
    }

    public async Task<string> CreateAsync(User user)
    {
        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        const string sql = @"
            INSERT INTO users (id, email, first_name, second_name, birthdate, gender, biography, city, password_hash)
            VALUES (@id, @email, @firstName, @secondName, @birthdate, @gender, @biography, @city, @passwordHash)";

        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", user.Id);
        cmd.Parameters.AddWithValue("@email", user.Email);
        cmd.Parameters.AddWithValue("@firstName", user.FirstName);
        cmd.Parameters.AddWithValue("@secondName", user.SecondName);
        cmd.Parameters.AddWithValue("@birthdate", user.Birthdate.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("@gender", user.Gender);
        cmd.Parameters.AddWithValue("@biography", user.Biography);
        cmd.Parameters.AddWithValue("@city", user.City);
        cmd.Parameters.AddWithValue("@passwordHash", user.PasswordHash);

        await cmd.ExecuteNonQueryAsync();
        return user.Id;
    }

    public async Task<User?> GetByIdAsync(string id)
    {
        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        const string sql = "SELECT id, email, first_name, second_name, birthdate, gender, biography, city, password_hash FROM users WHERE id = @id";

        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        return MapUser(reader);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        const string sql = "SELECT id, email, first_name, second_name, birthdate, gender, biography, city, password_hash FROM users WHERE email = @email";

        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@email", email.ToLower());

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        return MapUser(reader);
    }

    public async Task<List<User>> SearchAsync(string firstName, string lastName)
    {
        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        const string sql = @"
            SELECT id, email, first_name, second_name, birthdate, gender, biography, city, password_hash
            FROM users FORCE INDEX (idx_name_search)
            WHERE first_name LIKE @firstName AND second_name LIKE @lastName
            ORDER BY id";

        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@firstName", firstName + "%");
        cmd.Parameters.AddWithValue("@lastName", lastName + "%");

        var results = new List<User>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            results.Add(MapUser(reader));

        return results;
    }

    private static User MapUser(MySqlDataReader reader) => new()
    {
        Id = reader.GetString("id"),
        Email = reader.GetString("email"),
        FirstName = reader.GetString("first_name"),
        SecondName = reader.GetString("second_name"),
        Birthdate = DateOnly.FromDateTime(reader.GetDateTime("birthdate")),
        Gender = reader.IsDBNull(reader.GetOrdinal("gender")) ? string.Empty : reader.GetString("gender"),
        Biography = reader.IsDBNull(reader.GetOrdinal("biography")) ? string.Empty : reader.GetString("biography"),
        City = reader.GetString("city"),
        PasswordHash = reader.GetString("password_hash")
    };
}

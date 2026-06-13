using SocialNetwork.Models;

namespace SocialNetwork.Repositories;

public interface IUserRepository
{
    Task<string> CreateAsync(User user);
    Task<User?> GetByIdAsync(string id);
    Task<User?> GetByEmailAsync(string email);
    Task<List<User>> SearchAsync(string firstName, string lastName);
}

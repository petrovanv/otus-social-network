namespace SocialNetwork.Models;

public class User
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string SecondName { get; set; } = string.Empty;
    public DateOnly Birthdate { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string Biography { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
}

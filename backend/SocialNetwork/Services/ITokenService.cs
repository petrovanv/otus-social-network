namespace SocialNetwork.Services;

public interface ITokenService
{
    string GenerateToken(string userId);
}

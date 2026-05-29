using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Models;
using SocialNetwork.Repositories;
using SocialNetwork.Services;

namespace SocialNetwork.Controllers;

[ApiController]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;

    public AuthController(IUserRepository userRepository, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
    }

    /// <summary>Авторизация пользователя по email и паролю</summary>
    [HttpPost("/login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "Неверный email или пароль" });

        var token = _tokenService.GenerateToken(user.Id);
        return Ok(new { token, user_id = user.Id });
    }
}

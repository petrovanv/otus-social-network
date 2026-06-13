using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Models;
using SocialNetwork.Repositories;

namespace SocialNetwork.Controllers;

[ApiController]
[Route("user")]
public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public UserController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>Регистрация нового пользователя</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!DateOnly.TryParse(request.Birthdate, out var birthdate))
            return BadRequest(new { message = "Неверный формат даты рождения. Используйте YYYY-MM-DD" });

        if (await _userRepository.GetByEmailAsync(request.Email) != null)
            return Conflict(new { message = "Пользователь с таким email уже существует" });

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = request.Email.ToLower(),
            FirstName = request.FirstName,
            SecondName = request.SecondName,
            Birthdate = birthdate,
            Gender = request.Gender,
            Biography = request.Biography,
            City = request.City,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        var userId = await _userRepository.CreateAsync(user);
        return Ok(new { user_id = userId });
    }

    /// <summary>Поиск анкет по префиксу имени и фамилии</summary>
    [HttpGet("search")]
    [Authorize]
    public async Task<IActionResult> Search([FromQuery] string first_name, [FromQuery] string last_name)
    {
        if (string.IsNullOrWhiteSpace(first_name) || string.IsNullOrWhiteSpace(last_name))
            return BadRequest(new { message = "Параметры first_name и last_name обязательны" });

        var users = await _userRepository.SearchAsync(first_name, last_name);

        return Ok(users.Select(u => new UserResponse
        {
            Id = u.Id,
            Email = u.Email,
            FirstName = u.FirstName,
            SecondName = u.SecondName,
            Birthdate = u.Birthdate.ToString("yyyy-MM-dd"),
            Gender = u.Gender,
            Biography = u.Biography,
            City = u.City
        }));
    }

    /// <summary>Получение анкеты пользователя по ID</summary>
    [HttpGet("get/{id}")]
    [Authorize]
    public async Task<IActionResult> GetUser(string id)
    {
        var user = await _userRepository.GetByIdAsync(id);

        if (user == null)
            return NotFound(new { message = $"Пользователь {id} не найден" });

        return Ok(new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            SecondName = user.SecondName,
            Birthdate = user.Birthdate.ToString("yyyy-MM-dd"),
            Gender = user.Gender,
            Biography = user.Biography,
            City = user.City
        });
    }
}

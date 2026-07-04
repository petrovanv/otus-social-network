using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Repositories;
using SocialNetwork.Services;
using System.Security.Claims;

namespace SocialNetwork.Controllers;

[ApiController]
[Route("friend")]
[Authorize]
public class FriendController : ControllerBase
{
    private readonly IFriendRepository _friends;
    private readonly IFeedCache _feed;

    public FriendController(IFriendRepository friends, IFeedCache feed)
    {
        _friends = friends;
        _feed = feed;
    }

    private string CurrentUserId => User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

    /// <summary>Добавить пользователя в друзья (видеть его посты в ленте)</summary>
    [HttpPut("set/{user_id}")]
    public async Task<IActionResult> Set(string user_id)
    {
        if (user_id == CurrentUserId)
            return BadRequest(new { message = "Нельзя добавить в друзья самого себя" });

        await _friends.AddAsync(CurrentUserId, user_id);
        // состав друзей изменился — сбросим ленту, чтобы пересобралась с постами нового друга
        await _feed.InvalidateAsync([CurrentUserId]);
        return Ok(new { message = "Друг добавлен" });
    }

    /// <summary>Удалить пользователя из друзей</summary>
    [HttpPut("delete/{user_id}")]
    public async Task<IActionResult> Delete(string user_id)
    {
        await _friends.DeleteAsync(CurrentUserId, user_id);
        await _feed.InvalidateAsync([CurrentUserId]);
        return Ok(new { message = "Друг удалён" });
    }
}

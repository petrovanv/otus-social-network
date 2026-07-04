using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Models;
using SocialNetwork.Repositories;
using SocialNetwork.Services;
using System.Security.Claims;

namespace SocialNetwork.Controllers;

[ApiController]
[Route("post")]
[Authorize]
public class PostController : ControllerBase
{
    private readonly IPostRepository _posts;
    private readonly IFriendRepository _friends;
    private readonly IFeedCache _feed;

    public PostController(IPostRepository posts, IFriendRepository friends, IFeedCache feed)
    {
        _posts = posts;
        _friends = friends;
        _feed = feed;
    }

    private string CurrentUserId => User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

    /// <summary>Создать пост. Пост раскладывается по лентам подписчиков (fan-out).</summary>
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreatePostRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
            return BadRequest(new { message = "Текст поста не может быть пустым" });

        var post = await _posts.CreateAsync(CurrentUserId, request.Text);

        // fan-out on write: кладём пост в кэш-ленты всех, кто подписан на автора
        var followers = await _friends.GetFollowerIdsAsync(CurrentUserId);
        await _feed.PushToFollowersAsync(post, followers);

        return Ok(new { id = post.Id });
    }

    /// <summary>Обновить свой пост</summary>
    [HttpPut("update")]
    public async Task<IActionResult> Update([FromBody] UpdatePostRequest request)
    {
        var ok = await _posts.UpdateAsync(request.Id, CurrentUserId, request.Text);
        if (!ok) return NotFound(new { message = "Пост не найден или не принадлежит вам" });

        // текст изменился — сбросим ленты подписчиков, пересоберутся из БД
        var followers = await _friends.GetFollowerIdsAsync(CurrentUserId);
        await _feed.InvalidateAsync(followers);
        return Ok(new { message = "Пост обновлён" });
    }

    /// <summary>Удалить свой пост</summary>
    [HttpPut("delete")]
    public async Task<IActionResult> Delete([FromBody] DeletePostRequest request)
    {
        var deleted = await _posts.DeleteAsync(request.Id, CurrentUserId);
        if (deleted == null) return NotFound(new { message = "Пост не найден или не принадлежит вам" });

        var followers = await _friends.GetFollowerIdsAsync(CurrentUserId);
        await _feed.InvalidateAsync(followers);
        return Ok(new { message = "Пост удалён" });
    }

    /// <summary>Получить пост по ID</summary>
    [HttpGet("get/{id}")]
    public async Task<IActionResult> Get(string id)
    {
        var post = await _posts.GetByIdAsync(id);
        if (post == null) return NotFound(new { message = "Пост не найден" });
        return Ok(new { id = post.Id, text = post.Text, author_user_id = post.AuthorUserId });
    }

    /// <summary>Лента постов друзей (из кэша, последние 1000 обновлений)</summary>
    [HttpGet("feed")]
    public async Task<IActionResult> Feed([FromQuery] int offset = 0, [FromQuery] int limit = 10)
    {
        if (limit is < 1 or > 1000) limit = 10;
        if (offset < 0) offset = 0;

        var posts = await _feed.GetFeedAsync(
            CurrentUserId, offset, limit,
            rebuild: async () =>
            {
                var friendIds = await _friends.GetFriendIdsAsync(CurrentUserId);
                return await _posts.GetFeedFromDbAsync(friendIds, 1000);
            });

        return Ok(posts.Select(p => new
        {
            id = p.Id,
            text = p.Text,
            author_user_id = p.AuthorUserId
        }));
    }
}

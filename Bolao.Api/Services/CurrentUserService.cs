using System.Security.Claims;
using Bolao.Application.Common.Interfaces;

namespace Bolao.Api.Services;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? FirebaseUid =>
        _httpContextAccessor.HttpContext?.User.FindFirstValue("firebase_uid")
        ?? _httpContextAccessor.HttpContext?.User.FindFirstValue("user_id")
        ?? _httpContextAccessor.HttpContext?.User.FindFirstValue("uid")
        ?? _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
}
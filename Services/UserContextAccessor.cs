using System.Security.Claims;

namespace ReportedUsersSystem.Services;

public class UserContextAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContextAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetCurrentUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                   user.FindFirst("sub")?.Value;
        }
        return null;
    }

    public string? GetCurrentUserEmail()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            return user.FindFirst(ClaimTypes.Email)?.Value;
        }
        return null;
    }

    public string? GetCurrentUserName()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            return user.FindFirst(ClaimTypes.Name)?.Value;
        }
        return null;
    }

    public bool IsAuthenticated()
    {
        return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;
    }

    public bool IsCurrentUser(string userId)
    {
        var currentUserId = GetCurrentUserId();
        return !string.IsNullOrEmpty(currentUserId) && currentUserId == userId;
    }
}

using System.Security.Claims;

namespace StudyBuddy.Web.Utils;

public static class ClaimsExtensions
{
    /// <summary>Id of the logged-in user, taken from the auth cookie (never from the request body or query).</summary>
    public static int GetUserId(this ClaimsPrincipal user)
    {
        return int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
namespace RaceDay.Api.Auth;
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class SessionAuthorizeAttribute(string? role = null) : Attribute, IAuthorizationFilter {
    public void OnAuthorization(AuthorizationFilterContext context) {
        var session = context.HttpContext.Session;
        if (session.GetInt32("UserId") is null) { context.Result = new UnauthorizedObjectResult(new { message = "Please log in." }); return; }
        if (role is not null && !string.Equals(role, session.GetString("Role"), StringComparison.Ordinal))
            context.Result = new ObjectResult(new { message = "This action is not available to your account." }) { StatusCode = 403 };
    }
}
public static class SessionUser {
    public static int Id(HttpContext context) => context.Session.GetInt32("UserId") ?? 0;
    public static string Role(HttpContext context) => context.Session.GetString("Role") ?? "";
}

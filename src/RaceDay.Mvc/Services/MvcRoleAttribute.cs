using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
namespace RaceDay.Mvc.Services;
[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
public sealed class MvcRoleAttribute(string? role=null):ActionFilterAttribute {
    public override void OnActionExecuting(ActionExecutingContext context) {
        var s=context.HttpContext.Session;
        if(s.GetInt32("UserId") is null || string.IsNullOrWhiteSpace(s.GetString("ApiCookie")))
            context.Result=new RedirectToActionResult("Login","Account",null);
        else if(role is not null && s.GetString("Role")!=role)
            context.Result=new RedirectToActionResult("Index","Home",null);
    }
}

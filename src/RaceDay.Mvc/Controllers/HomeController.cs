using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.Mvc.Services;
namespace RaceDay.Mvc.Controllers;
public sealed class HomeController(ApiGateway api):Controller {
    public async Task<IActionResult> Index(string? search,string? type) {
        ViewBag.Search=search;ViewBag.Type=type;
        var url="api/events?upcoming=true";
        if(!string.IsNullOrWhiteSpace(search))url+="&search="+Uri.EscapeDataString(search);
        if(!string.IsNullOrWhiteSpace(type))url+="&type="+Uri.EscapeDataString(type);
        var result=await api.GetAsync<List<EventDto>>(url);
        if(!result.Ok)ViewBag.Error=result.Error;
        return View(result.Value??[]);
    }
}

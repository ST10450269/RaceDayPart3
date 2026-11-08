using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;
namespace RaceDay.Mvc.Controllers;
[MvcRole("Organiser")]
public sealed class DashboardController(ApiGateway api):Controller {
    public async Task<IActionResult> Index(){
        var r=await api.GetAsync<List<EventDto>>("api/events/mine");
        if(!r.Ok)TempData["Error"]=r.Error;
        var items=r.Value??[];
        return View(new OrganiserDashboardViewModel { Events=items,EnrolmentCounts=items.ToDictionary(x=>x.Id,x=>x.EnrolmentCount) });
    }
}

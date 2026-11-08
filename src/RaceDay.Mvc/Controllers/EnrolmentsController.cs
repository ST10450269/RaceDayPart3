using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.Mvc.Services;
namespace RaceDay.Mvc.Controllers;
[MvcRole]
public sealed class EnrolmentsController(ApiGateway api):Controller {
    [HttpPost,MvcRole("Participant")]public async Task<IActionResult> Enter(int eventId,int categoryId){var r=await api.SendAsync<object>(HttpMethod.Post,"api/enrolments",new EnrolRequest(eventId,categoryId));TempData[r.Ok?"Notice":"Error"]=r.Ok?"Your entry has been confirmed.":r.Error;return RedirectToAction("Details","Events",new{id=eventId});}
    [HttpGet,MvcRole("Participant")]public async Task<IActionResult> Mine(){var r=await api.GetAsync<List<EnrolmentDto>>("api/enrolments/mine");if(!r.Ok)TempData["Error"]=r.Error;return View(r.Value??[]);}
    [HttpGet,MvcRole("Organiser")]public async Task<IActionResult> Event(int id){var r=await api.GetAsync<List<EnrolmentDto>>($"api/events/{id}/enrolments");if(!r.Ok){TempData["Error"]=r.Error;return RedirectToAction("Index","Dashboard");}ViewBag.EventId=id;return View(r.Value??[]);}
    [HttpPost,MvcRole("Organiser")]public async Task<IActionResult> Status(int id,int eventId,string status){var r=await api.SendAsync<object>(HttpMethod.Put,$"api/enrolments/{id}/status",new EnrolmentStatusRequest(status));TempData[r.Ok?"Notice":"Error"]=r.Ok?"Status updated.":r.Error;return RedirectToAction(nameof(Event),new{id=eventId});}
}

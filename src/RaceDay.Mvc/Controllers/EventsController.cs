using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;
namespace RaceDay.Mvc.Controllers;
public sealed class EventsController(ApiGateway api):Controller {
    public async Task<IActionResult> Details(int id) {
        var e=await api.GetAsync<EventDto>($"api/events/{id}");if(!e.Ok||e.Value is null)return NotFound();
        var cats=await api.GetAsync<List<CategoryDto>>($"api/events/{id}/categories");
        return View(new EventDetailsViewModel{Event=e.Value,Categories=cats.Value??[]});
    }
    [HttpGet,MvcRole("Organiser")]public IActionResult Create()=>View("Edit",new EventForm());
    [HttpGet,MvcRole("Organiser")]public async Task<IActionResult> Edit(int id){var r=await api.GetAsync<EventDto>($"api/events/{id}");if(!r.Ok||r.Value is null)return NotFound();if(r.Value.OrganiserId!=HttpContext.Session.GetInt32("UserId"))return StatusCode(403,"This event does not belong to your account.");return View(EventForm.From(r.Value));}
    [HttpPost,MvcRole("Organiser")]public async Task<IActionResult> Save(EventForm form) {
        if(!ModelState.IsValid)return View("Edit",form);
        var r=form.Id==0?await api.SendAsync<EventDto>(HttpMethod.Post,"api/events",form.ToRequest()):await api.SendAsync<EventDto>(HttpMethod.Put,$"api/events/{form.Id}",form.ToRequest());
        if(!r.Ok){ModelState.AddModelError("",r.Error);return View("Edit",form);}TempData["Notice"]="Event saved.";
        return RedirectToAction("Index","Dashboard");
    }
    [HttpPost,MvcRole("Organiser")]public async Task<IActionResult> Delete(int id){var r=await api.SendAsync<object>(HttpMethod.Delete,$"api/events/{id}");TempData[r.Ok?"Notice":"Error"]=r.Ok?"Event deleted.":r.Error;return RedirectToAction("Index","Dashboard");}
    [HttpPost,MvcRole("Organiser")]public async Task<IActionResult> Banner(int id,IFormFile? file){if(file is null){TempData["Error"]="Choose an image.";return RedirectToAction(nameof(Details),new{id});}var r=await api.UploadAsync($"api/events/{id}/banner",file);TempData[r.Ok?"Notice":"Error"]=r.Ok?"Banner uploaded.":r.Error;return RedirectToAction(nameof(Details),new{id});}
}

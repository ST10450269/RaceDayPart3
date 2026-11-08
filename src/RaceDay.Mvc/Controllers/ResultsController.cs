using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;
namespace RaceDay.Mvc.Controllers;
[MvcRole]
public sealed class ResultsController(ApiGateway api):Controller {
    [HttpGet,MvcRole("Participant")]public async Task<IActionResult> Mine(){var r=await api.GetAsync<List<ResultDto>>("api/results/mine");if(!r.Ok)TempData["Error"]=r.Error;return View(r.Value??[]);}
    [HttpGet,MvcRole("Organiser")]public async Task<IActionResult> Event(int id){var e=await api.GetAsync<EventDto>($"api/events/{id}");if(!e.Ok||e.Value is null)return NotFound();var entries=await api.GetAsync<List<EnrolmentDto>>($"api/events/{id}/enrolments");var results=await api.GetAsync<List<ResultDto>>($"api/events/{id}/results");if(!entries.Ok||!results.Ok){TempData["Error"]=entries.Ok?results.Error:entries.Error;return RedirectToAction("Index","Dashboard");}return View(new EventResultsViewModel{Event=e.Value,Enrolments=entries.Value??[],Results=results.Value??[]});}
    [HttpPost,MvcRole("Organiser")]public async Task<IActionResult> Save(ResultForm form){
        if(!TimeSpan.TryParseExact(form.FinishTime,new[]{@"hh\:mm\:ss",@"h\:mm\:ss"},System.Globalization.CultureInfo.InvariantCulture,out var time)||time<=TimeSpan.Zero){TempData["Error"]="Use a valid finish time such as 01:15:30.";return RedirectToAction(nameof(Event),new{id=form.EventId});}
        var r=await api.SendAsync<object>(HttpMethod.Post,"api/results",new ResultWriteRequest(form.EnrolmentId,(int)time.TotalSeconds,form.Position,form.TotalFinishers));TempData[r.Ok?"Notice":"Error"]=r.Ok?"Result published.":r.Error;return RedirectToAction(nameof(Event),new{id=form.EventId});
    }
}

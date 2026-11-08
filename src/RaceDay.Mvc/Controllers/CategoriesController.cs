using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;
namespace RaceDay.Mvc.Controllers;
[MvcRole("Organiser")]
public sealed class CategoriesController(ApiGateway api):Controller {
    [HttpPost]public async Task<IActionResult> Create(CategoryForm form){
        if(!ModelState.IsValid){TempData["Error"]="Enter a valid category name.";return RedirectToAction("Details","Events",new{id=form.EventId});}
        var r=await api.SendAsync<CategoryDto>(HttpMethod.Post,$"api/events/{form.EventId}/categories",new CategoryWriteRequest(form.Name,form.MinAge,form.MaxAge,form.DistanceKm));
        TempData[r.Ok?"Notice":"Error"]=r.Ok?"Category added.":r.Error;return RedirectToAction("Details","Events",new{id=form.EventId});
    }
    [HttpPost]public async Task<IActionResult> Delete(int id,int eventId){var r=await api.SendAsync<object>(HttpMethod.Delete,$"api/categories/{id}");TempData[r.Ok?"Notice":"Error"]=r.Ok?"Category removed.":r.Error;return RedirectToAction("Details","Events",new{id=eventId});}
}

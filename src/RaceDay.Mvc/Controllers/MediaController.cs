using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Services;
namespace RaceDay.Mvc.Controllers;
public sealed class MediaController(ApiGateway api):Controller {
    // MVC proxies storage bytes through its API, rather than accessing Azure directly.
    [HttpGet]public async Task<IActionResult> Banner(int id){return await Stream($"api/events/{id}/banner");}
    [HttpGet,MvcRole]public async Task<IActionResult> Profile(){return await Stream("api/profile/picture");}
    private async Task<IActionResult> Stream(string route){
        try{using var r=await api.RawAsync(HttpMethod.Get,route);if(!r.IsSuccessStatusCode)return NotFound();var bytes=await r.Content.ReadAsByteArrayAsync();return File(bytes,r.Content.Headers.ContentType?.MediaType??"image/jpeg");}
        catch(HttpRequestException){return NotFound();}
    }
}

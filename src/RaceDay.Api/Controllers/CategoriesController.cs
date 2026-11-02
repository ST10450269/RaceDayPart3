using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Auth;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Contracts;
namespace RaceDay.Api.Controllers;
[ApiController,Route("api")]
public sealed class CategoriesController(RaceDayDbContext db):ControllerBase {
    private static CategoryDto Dto(Category c)=>new(c.Id,c.EventId,c.Name,c.MinAge,c.MaxAge,c.DistanceKm);
    /// <summary>List the available age or distance categories for an event.</summary>
    [HttpGet("events/{eventId:int}/categories")]
    public async Task<ActionResult<List<CategoryDto>>> All(int eventId) {if(!await db.Events.AnyAsync(x=>x.Id==eventId))return NotFound();return await db.Categories.Where(x=>x.EventId==eventId).OrderBy(x=>x.Name).Select(x=>new CategoryDto(x.Id,x.EventId,x.Name,x.MinAge,x.MaxAge,x.DistanceKm)).ToListAsync();}
    /// <summary>Create a category for your event.</summary>
    [HttpPost("events/{eventId:int}/categories"),SessionAuthorize("Organiser")]
    public async Task<IActionResult> Create(int eventId,CategoryWriteRequest req) {
        var e=await db.Events.FindAsync(eventId);if(e is null)return NotFound();if(e.OrganiserId!=SessionUser.Id(HttpContext))return StatusCode(403,new { message="You can only manage resources you own." });
        if(req.MinAge<0||req.MaxAge<0||(req.MinAge.HasValue&&req.MaxAge.HasValue&&req.MinAge>req.MaxAge)||req.DistanceKm<=0)return BadRequest(new{message="Category age or distance is invalid."});
        if(await db.Categories.AnyAsync(x=>x.EventId==eventId&&x.Name==req.Name))return Conflict(new{message="Category already exists."});
        var c=new Category{EventId=eventId,Name=req.Name.Trim(),MinAge=req.MinAge,MaxAge=req.MaxAge,DistanceKm=req.DistanceKm};db.Categories.Add(c);await db.SaveChangesAsync();return StatusCode(201,Dto(c));
    }
    /// <summary>Update one of your event's categories.</summary>
    [HttpPut("categories/{id:int}"),SessionAuthorize("Organiser")]
    public async Task<IActionResult> Update(int id,CategoryWriteRequest req) {
        var c=await db.Categories.Include(x=>x.Event).FirstOrDefaultAsync(x=>x.Id==id);if(c is null)return NotFound();if(c.Event.OrganiserId!=SessionUser.Id(HttpContext))return StatusCode(403,new { message="You can only manage resources you own." });
        if(req.MinAge<0||req.MaxAge<0||(req.MinAge.HasValue&&req.MaxAge.HasValue&&req.MinAge>req.MaxAge)||req.DistanceKm<=0)return BadRequest(new{message="Category age or distance is invalid."});
        if(await db.Categories.AnyAsync(x=>x.EventId==c.EventId&&x.Id!=id&&x.Name==req.Name))return Conflict(new{message="Category already exists."});
        c.Name=req.Name.Trim();c.MinAge=req.MinAge;c.MaxAge=req.MaxAge;c.DistanceKm=req.DistanceKm;await db.SaveChangesAsync();return NoContent();
    }
    /// <summary>Remove a category that does not have enrolments.</summary>
    [HttpDelete("categories/{id:int}"),SessionAuthorize("Organiser")]
    public async Task<IActionResult> Delete(int id) {
        var c=await db.Categories.Include(x=>x.Event).FirstOrDefaultAsync(x=>x.Id==id);if(c is null)return NotFound();if(c.Event.OrganiserId!=SessionUser.Id(HttpContext))return StatusCode(403,new { message="You can only manage resources you own." });
        if(await db.Enrolments.AnyAsync(x=>x.CategoryId==id))return Conflict(new{message="Category has enrolments."});
        db.Categories.Remove(c);await db.SaveChangesAsync();return NoContent();
    }
}

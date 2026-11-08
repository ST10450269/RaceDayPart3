using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Auth;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Api.Storage;
using RaceDay.Contracts;
namespace RaceDay.Api.Controllers;
[ApiController,Route("api/events")]
public sealed class EventsController(RaceDayDbContext db,BlobImageStore storage):ControllerBase {
    private static EventDto ToDto(RaceEvent e)=>new(e.Id,e.OrganiserId,e.Name,e.Description,e.DateUtc,e.Location,e.DistanceKm,e.EventType,e.BannerImageName is not null,e.Enrolments.Count);
    /// <summary>Browse events, optionally filtering by search, type and upcoming dates.</summary>
    [HttpGet]
    public async Task<ActionResult<List<EventDto>>> GetAll([FromQuery]string? search,[FromQuery]string? type,[FromQuery]bool upcoming=false) {
        var q=db.Events.Include(x=>x.Enrolments).AsNoTracking().AsQueryable();
        if(!string.IsNullOrWhiteSpace(search)) q=q.Where(x=>x.Name.Contains(search)||x.Location.Contains(search));
        if(!string.IsNullOrWhiteSpace(type)) q=q.Where(x=>x.EventType==type);
        if(upcoming) q=q.Where(x=>x.DateUtc>=DateTime.UtcNow);
        return (await q.OrderBy(x=>x.DateUtc).ToListAsync()).Select(ToDto).ToList();
    }
    /// <summary>Get event details and enrolment count.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<EventDto>> Get(int id) {var e=await db.Events.Include(x=>x.Enrolments).AsNoTracking().FirstOrDefaultAsync(x=>x.Id==id);return e is null?NotFound():ToDto(e);}
    /// <summary>List events owned by the signed-in Organiser.</summary>
    [HttpGet("mine"),SessionAuthorize("Organiser")]
    public async Task<ActionResult<List<EventDto>>> Mine() {var id=SessionUser.Id(HttpContext);return (await db.Events.Include(x=>x.Enrolments).AsNoTracking().Where(x=>x.OrganiserId==id).OrderBy(x=>x.DateUtc).ToListAsync()).Select(ToDto).ToList();}
    private IActionResult? Validate(EventWriteRequest req) {
        if(req.DateUtc==default)return BadRequest(new{message="Select an event date."});
        if(req.EventType is not ("Run" or "Walk" or "Cycle"))return BadRequest(new{message="Choose Run, Walk or Cycle."});
        return null;
    }
    /// <summary>Create a new race event (Organiser only).</summary>
    [HttpPost,SessionAuthorize("Organiser")]
    [ProducesResponseType(typeof(EventDto),201)]
    public async Task<IActionResult> Create(EventWriteRequest req) {
        if(Validate(req) is {} error)return error;
        var e=new RaceEvent { Name=req.Name.Trim(),Description=req.Description.Trim(),DateUtc=req.DateUtc.ToUniversalTime(),Location=req.Location.Trim(),DistanceKm=req.DistanceKm,EventType=req.EventType,OrganiserId=SessionUser.Id(HttpContext) };
        db.Events.Add(e);await db.SaveChangesAsync();return CreatedAtAction(nameof(Get),new {id=e.Id},ToDto(e));
    }
    /// <summary>Edit only an event you own.</summary>
    [HttpPut("{id:int}"),SessionAuthorize("Organiser")]
    public async Task<IActionResult> Update(int id,EventWriteRequest req) {
        if(Validate(req) is {} error)return error;
        var e=await db.Events.FindAsync(id);if(e is null)return NotFound();if(e.OrganiserId!=SessionUser.Id(HttpContext))return StatusCode(403,new { message="You can only manage resources you own." });
        e.Name=req.Name.Trim();e.Description=req.Description.Trim();e.DateUtc=req.DateUtc.ToUniversalTime();e.Location=req.Location.Trim();e.DistanceKm=req.DistanceKm;e.EventType=req.EventType;
        await db.SaveChangesAsync();return NoContent();
    }
    /// <summary>Delete your event if it has no participant enrolments.</summary>
    [HttpDelete("{id:int}"),SessionAuthorize("Organiser")]
    public async Task<IActionResult> Delete(int id) {
        var e=await db.Events.FindAsync(id);if(e is null)return NotFound();if(e.OrganiserId!=SessionUser.Id(HttpContext))return StatusCode(403,new { message="You can only manage resources you own." });
        if(await db.Enrolments.AnyAsync(x=>x.EventId==id))return Conflict(new {message="You cannot delete an event that has enrolments."});
        db.Events.Remove(e);await db.SaveChangesAsync();return NoContent();
    }
    /// <summary>Upload an event banner through the API to Azure Blob Storage.</summary>
    [HttpPost("{id:int}/banner"),SessionAuthorize("Organiser"),RequestSizeLimit(6_000_000)]
    public async Task<IActionResult> UploadBanner(int id,IFormFile file,CancellationToken token) {
        var e=await db.Events.FindAsync(id);if(e is null)return NotFound();if(e.OrganiserId!=SessionUser.Id(HttpContext))return StatusCode(403,new { message="You can only manage resources you own." });
        try { e.BannerImageName=await storage.UploadAsync(file,"raceday-banners",token); await db.SaveChangesAsync(token); return Ok(new{uploaded=true}); }
        catch(ArgumentException ex){return BadRequest(new{message=ex.Message});}
        catch(Azure.RequestFailedException){return StatusCode(503,new{message="Image storage is unavailable."});}
        catch(InvalidOperationException){return StatusCode(503,new{message="Image storage is not configured."});}
    }
    /// <summary>Get an event's public banner image.</summary>
    [HttpGet("{id:int}/banner")]
    public async Task<IActionResult> GetBanner(int id,CancellationToken token) {
        var name=await db.Events.Where(x=>x.Id==id).Select(x=>x.BannerImageName).FirstOrDefaultAsync(token);
        try {var found=await storage.GetAsync("raceday-banners",name,token);return found is null?NotFound():File(found.Value.Stream,found.Value.ContentType);}
        catch(Azure.RequestFailedException){return StatusCode(503);}
    }
}

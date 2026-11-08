using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Auth;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Contracts;
namespace RaceDay.Api.Controllers;
[ApiController,Route("api")]
public sealed class ResultsController(RaceDayDbContext db):ControllerBase {
    private static IQueryable<ResultDto> Select(IQueryable<RaceResult> q)=>q.Select(x=>new ResultDto(x.Id,x.EnrolmentId,x.Enrolment.EventId,x.Enrolment.Event.Name,x.Enrolment.Event.DateUtc,x.Enrolment.Category.Name,x.Enrolment.Participant.FullName,x.FinishTimeSeconds,x.Position,x.TotalFinishers,x.PublishedAtUtc));
    /// <summary>Publish or update a finish time and race position after an event ends.</summary>
    [HttpPost("results"),SessionAuthorize("Organiser")]
    public async Task<IActionResult> Save(ResultWriteRequest req) {
        if(req.Position>req.TotalFinishers)return BadRequest(new{message="Finishing position cannot exceed total finishers."});
        var entry=await db.Enrolments.Include(x=>x.Event).FirstOrDefaultAsync(x=>x.Id==req.EnrolmentId);if(entry is null)return NotFound();
        if(entry.Event.OrganiserId!=SessionUser.Id(HttpContext))return StatusCode(403,new { message="You can only manage resources you own." });
        if(entry.Event.DateUtc>DateTime.UtcNow)return Conflict(new{message="Results can only be recorded after the event date."});
        if(entry.Status!="Confirmed")return Conflict(new{message="Only confirmed participants can receive results."});
        var r=await db.Results.SingleOrDefaultAsync(x=>x.EnrolmentId==req.EnrolmentId);
        if(r is null){r=new RaceResult{EnrolmentId=req.EnrolmentId};db.Results.Add(r);}
        r.FinishTimeSeconds=req.FinishTimeSeconds;r.Position=req.Position;r.TotalFinishers=req.TotalFinishers;r.PublishedAtUtc=DateTime.UtcNow;
        await db.SaveChangesAsync();return Ok(new{id=r.Id, saved=true});
    }
    /// <summary>See your own completed race results.</summary>
    [HttpGet("results/mine"),SessionAuthorize("Participant")]
    public async Task<List<ResultDto>> Mine() {var id=SessionUser.Id(HttpContext);return await Select(db.Results.AsNoTracking().Where(x=>x.Enrolment.ParticipantId==id).OrderByDescending(x=>x.Enrolment.Event.DateUtc)).ToListAsync();}
    /// <summary>See published results for an event you organise.</summary>
    [HttpGet("events/{eventId:int}/results"),SessionAuthorize("Organiser")]
    public async Task<ActionResult<List<ResultDto>>> EventResults(int eventId) {
        var e=await db.Events.FindAsync(eventId);if(e is null)return NotFound();if(e.OrganiserId!=SessionUser.Id(HttpContext))return StatusCode(403,new { message="You can only manage resources you own." });
        return await Select(db.Results.AsNoTracking().Where(x=>x.Enrolment.EventId==eventId)).OrderBy(x=>x.Position).ToListAsync();
    }
}

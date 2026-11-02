using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Auth;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Contracts;
namespace RaceDay.Api.Controllers;
[ApiController,Route("api")]
public sealed class EnrolmentsController(RaceDayDbContext db):ControllerBase {
    private static IQueryable<EnrolmentDto> Select(IQueryable<Enrolment> query)=>query.Select(x=>new EnrolmentDto(x.Id,x.EventId,x.Event.Name,x.Event.DateUtc,x.CategoryId,x.Category.Name,x.ParticipantId,x.Participant.FullName,x.Participant.Email,x.Status,x.EnteredAtUtc));
    /// <summary>Enter an upcoming event as a Participant, choosing a category.</summary>
    [HttpPost("enrolments"),SessionAuthorize("Participant")]
    public async Task<IActionResult> Enter(EnrolRequest req) {
        var e=await db.Events.FindAsync(req.EventId);if(e is null)return NotFound(new{message="Event not found."});
        if(e.DateUtc<=DateTime.UtcNow)return Conflict(new{message="You cannot enter an event that has finished."});
        if(!await db.Categories.AnyAsync(x=>x.Id==req.CategoryId&&x.EventId==req.EventId))return BadRequest(new{message="Select a category belonging to this event."});
        var userId=SessionUser.Id(HttpContext);
        if(await db.Enrolments.AnyAsync(x=>x.EventId==req.EventId&&x.ParticipantId==userId))return Conflict(new{message="You are already entered for this event."});
        var enrol=new Enrolment{EventId=req.EventId,CategoryId=req.CategoryId,ParticipantId=userId,Status="Confirmed"};db.Enrolments.Add(enrol);await db.SaveChangesAsync();return StatusCode(201,new{id=enrol.Id,status=enrol.Status});
    }
    /// <summary>Show the current Participant's own race entries.</summary>
    [HttpGet("enrolments/mine"),SessionAuthorize("Participant")]
    public async Task<List<EnrolmentDto>> Mine() {var id=SessionUser.Id(HttpContext);return await Select(db.Enrolments.AsNoTracking().Where(x=>x.ParticipantId==id)).OrderBy(x=>x.EventDateUtc).ToListAsync();}
    /// <summary>List the participants entered for one of your events.</summary>
    [HttpGet("events/{eventId:int}/enrolments"),SessionAuthorize("Organiser")]
    public async Task<ActionResult<List<EnrolmentDto>>> ByEvent(int eventId) {
        var e=await db.Events.FindAsync(eventId);if(e is null)return NotFound();if(e.OrganiserId!=SessionUser.Id(HttpContext))return StatusCode(403,new { message="You can only manage resources you own." });
        return await Select(db.Enrolments.AsNoTracking().Where(x=>x.EventId==eventId)).OrderBy(x=>x.ParticipantName).ToListAsync();
    }
    /// <summary>Confirm or mark pending an enrolment for your event.</summary>
    [HttpPut("enrolments/{id:int}/status"),SessionAuthorize("Organiser")]
    public async Task<IActionResult> SetStatus(int id,EnrolmentStatusRequest req) {
        if(req.Status is not ("Pending" or "Confirmed"))return BadRequest(new{message="Status must be Pending or Confirmed."});
        var enrol=await db.Enrolments.Include(x=>x.Event).FirstOrDefaultAsync(x=>x.Id==id);
        if(enrol is null)return NotFound();if(enrol.Event.OrganiserId!=SessionUser.Id(HttpContext))return StatusCode(403,new { message="You can only manage resources you own." });
        enrol.Status=req.Status;await db.SaveChangesAsync();return NoContent();
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Auth;
using RaceDay.Api.Data;
using RaceDay.Api.Storage;
using RaceDay.Contracts;
namespace RaceDay.Api.Controllers;
[ApiController,Route("api/profile"),SessionAuthorize]
public sealed class ProfileController(RaceDayDbContext db,BlobImageStore storage):ControllerBase {
    /// <summary>Read your own profile information.</summary>
    [HttpGet]
    public async Task<ActionResult<ProfileDto>> Get() {
        var u=await db.Users.Include(x=>x.Role).SingleAsync(x=>x.Id==SessionUser.Id(HttpContext));
        return new ProfileDto(u.Id,u.FullName,u.Email,u.Phone,u.City,u.Role.Name,u.ProfileImageName is not null);
    }
    /// <summary>Update your own name, phone and city.</summary>
    [HttpPut]
    public async Task<IActionResult> Update(ProfileUpdateRequest req) {
        var u=await db.Users.FindAsync(SessionUser.Id(HttpContext)); if(u is null)return Unauthorized();
        u.FullName=req.FullName.Trim();u.Phone=req.Phone.Trim();u.City=req.City.Trim();await db.SaveChangesAsync();return NoContent();
    }
    /// <summary>Upload a JPG/PNG/WEBP profile picture to Azure Blob Storage via this API.</summary>
    [HttpPost("picture"),RequestSizeLimit(6_000_000)]
    public async Task<IActionResult> Picture(IFormFile file,CancellationToken token) {
        var u=await db.Users.FindAsync(SessionUser.Id(HttpContext)); if(u is null)return Unauthorized();
        try { u.ProfileImageName=await storage.UploadAsync(file,"raceday-profiles",token);await db.SaveChangesAsync(token);return Ok(new { uploaded=true }); }
        catch(ArgumentException ex) { return BadRequest(new { message=ex.Message }); }
        catch(Azure.RequestFailedException) { return StatusCode(503,new { message="Image storage is unavailable. Check Azure Blob configuration." }); }
        catch(InvalidOperationException) { return StatusCode(503,new { message="Image storage is not configured." }); }
    }
    /// <summary>Stream your private profile picture from Azure Blob Storage.</summary>
    [HttpGet("picture")]
    public async Task<IActionResult> Picture(CancellationToken token) {
        var u=await db.Users.FindAsync(SessionUser.Id(HttpContext));if(u is null)return Unauthorized();
        try { var found=await storage.GetAsync("raceday-profiles",u.ProfileImageName,token); return found is null?NotFound():File(found.Value.Stream,found.Value.ContentType); }
        catch(Azure.RequestFailedException){return StatusCode(503);}
    }
}

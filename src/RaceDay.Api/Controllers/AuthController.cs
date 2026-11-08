using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Auth;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Contracts;
namespace RaceDay.Api.Controllers;
[ApiController, Route("api/auth")]
public sealed class AuthController(RaceDayDbContext db, IPasswordHasher<AppUser> hasher) : ControllerBase {
    /// <summary>Register a new Participant or Organiser with a securely hashed password.</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse),201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Register(RegisterRequest request) {
        var roleName=request.Role.Trim();
        if(roleName is not ("Organiser" or "Participant")) return BadRequest(new { message="Choose Organiser or Participant." });
        var email=request.Email.Trim().ToLowerInvariant();
        if(await db.Users.AnyAsync(x=>x.Email==email)) return Conflict(new { message="That email address is already registered." });
        var role=await db.Roles.SingleAsync(x=>x.Name==roleName);
        var user=new AppUser { FullName=request.FullName.Trim(),Email=email,Phone=request.Phone.Trim(),City=request.City.Trim(),RoleId=role.Id };
        user.PasswordHash=hasher.HashPassword(user,request.Password);
        db.Users.Add(user); await db.SaveChangesAsync();
        return StatusCode(201,new AuthResponse(user.Id,user.FullName,user.Email,roleName));
    }
    /// <summary>Log in; the server stores your user ID and role in the session.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse),200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login(LoginRequest request) {
        var email=request.Email.Trim().ToLowerInvariant();
        var user=await db.Users.Include(x=>x.Role).SingleOrDefaultAsync(x=>x.Email==email);
        if(user is null || hasher.VerifyHashedPassword(user,user.PasswordHash,request.Password)==PasswordVerificationResult.Failed)
            return Unauthorized(new { message="Incorrect email or password." });
        HttpContext.Session.Clear();
        HttpContext.Session.SetInt32("UserId",user.Id);
        HttpContext.Session.SetString("Role",user.Role.Name);
        return Ok(new AuthResponse(user.Id,user.FullName,user.Email,user.Role.Name));
    }
    /// <summary>Inspect your current session (401 if not logged in).</summary>
    [HttpGet("session"),SessionAuthorize]
    public async Task<IActionResult> SessionInfo() {
        var u=await db.Users.Include(x=>x.Role).SingleOrDefaultAsync(x=>x.Id==SessionUser.Id(HttpContext));
        return u is null ? Unauthorized() : Ok(new AuthResponse(u.Id,u.FullName,u.Email,u.Role.Name));
    }
    /// <summary>End the current server-side session.</summary>
    [HttpPost("logout"),SessionAuthorize]
    public IActionResult Logout() { HttpContext.Session.Clear(); return NoContent(); }
}

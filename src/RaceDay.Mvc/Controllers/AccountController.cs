using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;
namespace RaceDay.Mvc.Controllers;
public sealed class AccountController(ApiGateway api):Controller {
    [HttpGet]public IActionResult Login()=>View(new LoginForm());
    [HttpPost]public async Task<IActionResult> Login(LoginForm form) {
        if(!ModelState.IsValid)return View(form);
        var (response,cookie)=await api.LoginAsync<AuthResponse>(new LoginRequest(form.Email,form.Password));
        if(!response.Ok||response.Value is null||cookie is null){ModelState.AddModelError("",response.Ok?"Session cookie could not be created.":response.Error);return View(form);}
        HttpContext.Session.Clear();var u=response.Value;
        HttpContext.Session.SetInt32("UserId",u.UserId);HttpContext.Session.SetString("FullName",u.FullName);HttpContext.Session.SetString("Role",u.Role);HttpContext.Session.SetString("ApiCookie",cookie);
        return RedirectToAction(u.Role=="Organiser"?"Index":"Index",u.Role=="Organiser"?"Dashboard":"Home");
    }
    [HttpGet]public IActionResult Register()=>View(new RegisterForm());
    [HttpPost]public async Task<IActionResult> Register(RegisterForm form) {
        if(!ModelState.IsValid)return View(form);
        var response=await api.SendAsync<AuthResponse>(HttpMethod.Post,"api/auth/register",new RegisterRequest(form.FullName,form.Email,form.Password,form.Role,form.Phone,form.City));
        if(!response.Ok){ModelState.AddModelError("",response.Error);return View(form);}
        TempData["Notice"]="Your account was created. You can now sign in.";return RedirectToAction(nameof(Login));
    }
    [HttpPost,MvcRole]public async Task<IActionResult> Logout(){await api.SendAsync<object>(HttpMethod.Post,"api/auth/logout");HttpContext.Session.Clear();return RedirectToAction("Index","Home");}
    [HttpGet,MvcRole]public async Task<IActionResult> Profile(){var r=await api.GetAsync<ProfileDto>("api/profile");if(!r.Ok||r.Value is null){TempData["Error"]=r.Error;return RedirectToAction("Index","Home");}var u=r.Value;return View(new ProfileForm{FullName=u.FullName,Phone=u.Phone,City=u.City,Email=u.Email,HasProfilePicture=u.HasProfilePicture});}
    [HttpPost,MvcRole]public async Task<IActionResult> Profile(ProfileForm form){if(!ModelState.IsValid)return View(form);var r=await api.SendAsync<object>(HttpMethod.Put,"api/profile",new ProfileUpdateRequest(form.FullName,form.Phone,form.City));if(!r.Ok){ModelState.AddModelError("",r.Error);return View(form);}HttpContext.Session.SetString("FullName",form.FullName);TempData["Notice"]="Profile updated.";return RedirectToAction(nameof(Profile));}
    [HttpPost,MvcRole]public async Task<IActionResult> Picture(IFormFile? file){if(file is null){TempData["Error"]="Select an image first.";return RedirectToAction(nameof(Profile));}var r=await api.UploadAsync("api/profile/picture",file);TempData[r.Ok?"Notice":"Error"]=r.Ok?"Profile image saved.":r.Error;return RedirectToAction(nameof(Profile));}
}

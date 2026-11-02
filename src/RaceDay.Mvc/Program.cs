using RaceDay.Mvc.Services;
var builder=WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews(o=>o.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute()));
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o=> {o.Cookie.Name="RaceDay.Mvc.Session";o.Cookie.HttpOnly=true;o.Cookie.SameSite=SameSiteMode.Lax;o.Cookie.IsEssential=true;o.IdleTimeout=TimeSpan.FromHours(4);});
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient("RaceDayApi",c=>{
    c.BaseAddress=new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5080/");
    c.Timeout=TimeSpan.FromSeconds(30);
}).ConfigurePrimaryHttpMessageHandler(()=>new HttpClientHandler { UseCookies=false });
builder.Services.AddScoped<ApiGateway>();
var app=builder.Build();
app.UseStaticFiles();app.UseRouting();app.UseSession();
app.MapControllerRoute(name:"default",pattern:"{controller=Home}/{action=Index}/{id?}");
app.Run();

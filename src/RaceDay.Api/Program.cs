using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Api.Storage;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opts => {
    opts.SwaggerDoc("v1",new OpenApiInfo { Title="RaceDay REST API",Version="v1",Description="Session-based RaceDay API. Log in using /api/auth/login in Swagger first; your browser retains the session cookie." });
    var xml=Path.Combine(AppContext.BaseDirectory,"RaceDay.Api.xml"); if(File.Exists(xml)) opts.IncludeXmlComments(xml);
});
builder.Services.AddDbContext<RaceDayDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("RaceDay"),sql=>sql.EnableRetryOnFailure(6)));
builder.Services.AddScoped<IPasswordHasher<AppUser>,PasswordHasher<AppUser>>();
builder.Services.AddScoped<BlobImageStore>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options => {
    options.Cookie.Name="RaceDay.Api.Session";
    options.Cookie.HttpOnly=true;
    options.Cookie.SameSite=SameSiteMode.Lax;
    options.Cookie.IsEssential=true;
    options.IdleTimeout=TimeSpan.FromHours(4);
});
var app=builder.Build();
app.UseSwagger(); app.UseSwaggerUI();
app.UseSession();
app.MapControllers();
app.MapGet("/health",()=>Results.Ok(new { status="ok" }));
if (!app.Environment.IsEnvironment("Testing")) {
    // SQL Server may need time to accept connections on first Docker startup.
    for(var attempt=1;attempt<=40;attempt++) {
        try { using var scope=app.Services.CreateScope(); await SeedData.ApplyAsync(scope.ServiceProvider.GetRequiredService<RaceDayDbContext>()); break; }
        catch(Exception ex) when(attempt<40 && (ex is Microsoft.Data.SqlClient.SqlException || ex is InvalidOperationException)) { app.Logger.LogWarning("Waiting for SQL Server (attempt {Attempt})",attempt); await Task.Delay(3000); }
    }
}
app.Run();
public partial class Program { }

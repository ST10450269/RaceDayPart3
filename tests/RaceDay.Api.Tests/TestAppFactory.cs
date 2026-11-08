using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RaceDay.Api.Data;
namespace RaceDay.Api.Tests;
public sealed class TestAppFactory:WebApplicationFactory<Program> {
    private readonly SqliteConnection _connection=new("Data Source=:memory:");
    public TestAppFactory(){_connection.Open();}
    protected override void ConfigureWebHost(IWebHostBuilder builder) {
        builder.UseEnvironment("Testing");
        builder.ConfigureTestServices(services=>{
            services.RemoveAll<DbContextOptions<RaceDayDbContext>>();
            services.AddDbContext<RaceDayDbContext>(o=>o.UseSqlite(_connection));
        });
    }
    public HttpClient NewClient() {
        var c=CreateClient(new WebApplicationFactoryClientOptions{HandleCookies=true});
        using var scope=Services.CreateScope();
        SeedData.ApplyAsync(scope.ServiceProvider.GetRequiredService<RaceDayDbContext>()).GetAwaiter().GetResult();
        return c;
    }
    protected override void Dispose(bool disposing){base.Dispose(disposing);if(disposing)_connection.Dispose();}
}

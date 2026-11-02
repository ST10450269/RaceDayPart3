using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;
namespace RaceDay.Api.Data;
public static class SeedData {
    public static async Task ApplyAsync(RaceDayDbContext db) {
        // Starter project strategy: create the schema from EF Core's Code-First model on an empty DB.
        // For production schema evolution, replace EnsureCreated with versioned EF migrations.
        await db.Database.EnsureCreatedAsync();
        if (await db.Roles.AnyAsync()) return;
        var organiserRole=new Role { Name="Organiser" }; var participantRole=new Role { Name="Participant" };
        db.Roles.AddRange(organiserRole,participantRole); await db.SaveChangesAsync();
        var hasher=new PasswordHasher<AppUser>();
        AppUser NewUser(string name,string email,string pw,Role role,string city) {
            var u=new AppUser { FullName=name, Email=email, RoleId=role.Id, Phone="0820000000", City=city };
            u.PasswordHash=hasher.HashPassword(u,pw); return u;
        }
        var org1=NewUser("Thandi Mokoena","organiser@raceday.test","DemoOrg123!",organiserRole,"Durban");
        var org2=NewUser("Peter Jacobs","organiser2@raceday.test","DemoOrg123!",organiserRole,"Cape Town");
        var p1=NewUser("Nomsa Dlamini","runner@raceday.test","DemoRunner123!",participantRole,"Pietermaritzburg");
        var p2=NewUser("Sizwe Nkosi","runner2@raceday.test","DemoRunner123!",participantRole,"Johannesburg");
        db.Users.AddRange(org1,org2,p1,p2); await db.SaveChangesAsync();
        var past=new RaceEvent { Name="Heritage Road Race",Description="Community road race through KwaZulu-Natal.",DateUtc=DateTime.UtcNow.AddDays(-30), Location="Pietermaritzburg",DistanceKm=10,EventType="Run",OrganiserId=org1.Id };
        var upcoming=new RaceEvent { Name="Durban Coastal Run",Description="A scenic coastal race for all fitness levels.",DateUtc=DateTime.UtcNow.AddDays(21),Location="Durban",DistanceKm=21,EventType="Run",OrganiserId=org1.Id };
        var cycle=new RaceEvent { Name="Cape Pedal Challenge",Description="A road cycling challenge along beautiful coastal roads.",DateUtc=DateTime.UtcNow.AddDays(40),Location="Cape Town",DistanceKm=50,EventType="Cycle",OrganiserId=org2.Id };
        db.Events.AddRange(past,upcoming,cycle); await db.SaveChangesAsync();
        var c1=new Category { EventId=past.Id,Name="10 km Open",DistanceKm=10 };
        var c2=new Category { EventId=upcoming.Id,Name="21 km Open",DistanceKm=21 };
        var c3=new Category { EventId=upcoming.Id,Name="Under 20",MinAge=12,MaxAge=19 };
        var c4=new Category { EventId=cycle.Id,Name="50 km Open",DistanceKm=50 };
        db.Categories.AddRange(c1,c2,c3,c4); await db.SaveChangesAsync();
        var e1=new Enrolment { EventId=past.Id, CategoryId=c1.Id,ParticipantId=p1.Id,Status="Confirmed" };
        var e2=new Enrolment { EventId=upcoming.Id,CategoryId=c2.Id,ParticipantId=p1.Id,Status="Confirmed" };
        var e3=new Enrolment { EventId=cycle.Id,CategoryId=c4.Id,ParticipantId=p2.Id,Status="Pending" };
        db.Enrolments.AddRange(e1,e2,e3); await db.SaveChangesAsync();
        db.Results.Add(new RaceResult { EnrolmentId=e1.Id,FinishTimeSeconds=3234,Position=47,TotalFinishers=312,PublishedAtUtc=DateTime.UtcNow.AddDays(-29) });
        await db.SaveChangesAsync();
    }
}

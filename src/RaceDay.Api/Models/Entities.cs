namespace RaceDay.Api.Models;
public sealed class Role { public int Id { get; set; } public string Name { get; set; } = ""; public List<AppUser> Users { get; set; } = []; }
public sealed class AppUser {
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Phone { get; set; } = "";
    public string City { get; set; } = "";
    public string? ProfileImageName { get; set; }
    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;
    public List<RaceEvent> Events { get; set; } = [];
    public List<Enrolment> Enrolments { get; set; } = [];
}
public sealed class RaceEvent {
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime DateUtc { get; set; }
    public string Location { get; set; } = "";
    public decimal DistanceKm { get; set; }
    public string EventType { get; set; } = "";
    public string? BannerImageName { get; set; }
    public int OrganiserId { get; set; }
    public AppUser Organiser { get; set; } = null!;
    public List<Category> Categories { get; set; } = [];
    public List<Enrolment> Enrolments { get; set; } = [];
}
public sealed class Category {
    public int Id { get; set; }
    public int EventId { get; set; }
    public RaceEvent Event { get; set; } = null!;
    public string Name { get; set; } = "";
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public decimal? DistanceKm { get; set; }
    public List<Enrolment> Enrolments { get; set; } = [];
}
public sealed class Enrolment {
    public int Id { get; set; }
    public int EventId { get; set; }
    public RaceEvent Event { get; set; } = null!;
    public int ParticipantId { get; set; }
    public AppUser Participant { get; set; } = null!;
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public string Status { get; set; } = "Confirmed";
    public DateTime EnteredAtUtc { get; set; } = DateTime.UtcNow;
    public RaceResult? Result { get; set; }
}
public sealed class RaceResult {
    public int Id { get; set; }
    public int EnrolmentId { get; set; }
    public Enrolment Enrolment { get; set; } = null!;
    public int FinishTimeSeconds { get; set; }
    public int Position { get; set; }
    public int TotalFinishers { get; set; }
    public DateTime PublishedAtUtc { get; set; } = DateTime.UtcNow;
}

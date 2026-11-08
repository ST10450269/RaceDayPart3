using System.ComponentModel.DataAnnotations;

namespace RaceDay.Contracts;
public sealed record RegisterRequest(
    [Required, StringLength(100, MinimumLength = 2)] string FullName,
    [Required, EmailAddress] string Email,
    [Required, MinLength(8), RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$", ErrorMessage = "Password must be at least 8 characters and include an uppercase letter, lowercase letter, number, and symbol.")] string Password,
    [Required] string Role,
    [Required, Phone] string Phone,
    [Required] string City);
public sealed record LoginRequest([Required, EmailAddress] string Email, [Required] string Password);
public sealed record AuthResponse(int UserId, string FullName, string Email, string Role);
public sealed record ProfileDto(int Id, string FullName, string Email, string Phone, string City, string Role, bool HasProfilePicture);
public sealed record ProfileUpdateRequest([Required] string FullName, [Required, Phone] string Phone, [Required] string City);
public sealed record EventDto(int Id, int OrganiserId, string Name, string Description, DateTime DateUtc, string Location, decimal DistanceKm, string EventType, bool HasBanner, int EnrolmentCount);
public sealed record EventWriteRequest(
    [Required, StringLength(120)] string Name,
    [Required, StringLength(1500)] string Description,
    DateTime DateUtc,
    [Required] string Location,
    [Range(0.1, 1000)] decimal DistanceKm,
    [Required] string EventType);
public sealed record CategoryDto(int Id, int EventId, string Name, int? MinAge, int? MaxAge, decimal? DistanceKm);
public sealed record CategoryWriteRequest([Required] string Name, int? MinAge, int? MaxAge, decimal? DistanceKm);
public sealed record EnrolRequest(int EventId, int CategoryId);
public sealed record EnrolmentDto(int Id, int EventId, string EventName, DateTime EventDateUtc, int CategoryId, string CategoryName, int ParticipantId, string ParticipantName, string ParticipantEmail, string Status, DateTime EnteredAtUtc);
public sealed record EnrolmentStatusRequest([Required] string Status);
public sealed record ResultWriteRequest(int EnrolmentId, [Range(1, 864000)] int FinishTimeSeconds, [Range(1, 1000000)] int Position, [Range(1, 1000000)] int TotalFinishers);
public sealed record ResultDto(int Id, int EnrolmentId, int EventId, string EventName, DateTime EventDateUtc, string CategoryName, string ParticipantName, int FinishTimeSeconds, int Position, int TotalFinishers, DateTime PublishedAtUtc);

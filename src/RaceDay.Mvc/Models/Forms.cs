using System.ComponentModel.DataAnnotations;
using RaceDay.Contracts;
namespace RaceDay.Mvc.Models;
public sealed class LoginForm { [Required, EmailAddress] public string Email {get;set;}="";[Required]public string Password{get;set;}=""; }
public sealed class RegisterForm { [Required]public string FullName{get;set;}="";[Required,EmailAddress]public string Email{get;set;}="";[Required,MinLength(8)]public string Password{get;set;}="";[Required]public string Role{get;set;}="Participant";[Required,Phone]public string Phone{get;set;}="";[Required]public string City{get;set;}=""; }
public sealed class ProfileForm { [Required]public string FullName{get;set;}="";[Required,Phone]public string Phone{get;set;}="";[Required]public string City{get;set;}="";public string Email{get;set;}="";public bool HasProfilePicture{get;set;} }
public sealed class EventForm { public int Id{get;set;}[Required]public string Name{get;set;}="";[Required]public string Description{get;set;}="";[Required]public string Location{get;set;}="";[Range(0.1,1000)]public decimal DistanceKm{get;set;}=10;[Required]public string EventType{get;set;}="Run";public DateTime DateUtc{get;set;}=DateTime.UtcNow.AddDays(14);public static EventForm From(EventDto e)=>new(){ Id=e.Id,Name=e.Name,Description=e.Description,Location=e.Location,DistanceKm=e.DistanceKm,EventType=e.EventType,DateUtc=e.DateUtc };public EventWriteRequest ToRequest()=>new(Name,Description,DateUtc,Location,DistanceKm,EventType); }
public sealed class CategoryForm { public int EventId{get;set;}[Required]public string Name{get;set;}="";public int? MinAge{get;set;}public int? MaxAge{get;set;}public decimal? DistanceKm{get;set;} }
public sealed class ResultForm { public int EventId{get;set;}public int EnrolmentId{get;set;}[Required]public string FinishTime{get;set;}="00:45:00";[Range(1,1000000)]public int Position{get;set;}=1;[Range(1,1000000)]public int TotalFinishers{get;set;}=1; }
public sealed class EventDetailsViewModel { public EventDto Event{get;set;}=null!;public List<CategoryDto> Categories{get;set;}=[]; }
public sealed class OrganiserDashboardViewModel { public List<EventDto> Events{get;set;}=[]; public Dictionary<int,int> EnrolmentCounts{get;set;}=[]; }
public sealed class EventResultsViewModel { public EventDto Event{get;set;}=null!; public List<EnrolmentDto> Enrolments{get;set;}=[];public List<ResultDto> Results{get;set;}=[]; }

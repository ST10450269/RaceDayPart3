using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;
namespace RaceDay.Api.Data;
public sealed class RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : DbContext(options) {
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<RaceEvent> Events => Set<RaceEvent>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<RaceResult> Results => Set<RaceResult>();
    protected override void OnModelCreating(ModelBuilder model) {
        model.Entity<Role>(b => { b.ToTable("Roles"); b.HasKey(x=>x.Id); b.Property(x=>x.Name).HasMaxLength(30).IsRequired(); b.HasIndex(x=>x.Name).IsUnique(); });
        model.Entity<AppUser>(b => { b.ToTable("Users"); b.HasKey(x=>x.Id); b.Property(x=>x.FullName).HasMaxLength(100).IsRequired(); b.Property(x=>x.Email).HasMaxLength(200).IsRequired(); b.HasIndex(x=>x.Email).IsUnique(); b.Property(x=>x.PasswordHash).HasMaxLength(500).IsRequired(); b.Property(x=>x.Phone).HasMaxLength(30).IsRequired(); b.Property(x=>x.City).HasMaxLength(100).IsRequired(); b.Property(x=>x.ProfileImageName).HasMaxLength(260); b.HasOne(x=>x.Role).WithMany(x=>x.Users).HasForeignKey(x=>x.RoleId).OnDelete(DeleteBehavior.Restrict); });
        model.Entity<RaceEvent>(b => { b.ToTable("Events"); b.HasKey(x=>x.Id); b.Property(x=>x.Name).HasMaxLength(120).IsRequired(); b.Property(x=>x.Description).HasMaxLength(1500).IsRequired(); b.Property(x=>x.Location).HasMaxLength(200).IsRequired(); b.Property(x=>x.EventType).HasMaxLength(12).IsRequired(); b.Property(x=>x.DistanceKm).HasPrecision(8,2); b.Property(x=>x.BannerImageName).HasMaxLength(260); b.HasOne(x=>x.Organiser).WithMany(x=>x.Events).HasForeignKey(x=>x.OrganiserId).OnDelete(DeleteBehavior.Restrict); });
        model.Entity<Category>(b => { b.ToTable("Categories"); b.HasKey(x=>x.Id); b.Property(x=>x.Name).HasMaxLength(100).IsRequired(); b.Property(x=>x.DistanceKm).HasPrecision(8,2); b.HasIndex(x=>new{x.EventId,x.Name}).IsUnique(); b.HasOne(x=>x.Event).WithMany(x=>x.Categories).HasForeignKey(x=>x.EventId).OnDelete(DeleteBehavior.Cascade); });
        model.Entity<Enrolment>(b => { b.ToTable("Enrolments"); b.HasKey(x=>x.Id); b.Property(x=>x.Status).HasMaxLength(20).HasDefaultValue("Confirmed"); b.HasIndex(x=>new{x.EventId,x.ParticipantId}).IsUnique(); b.HasOne(x=>x.Event).WithMany(x=>x.Enrolments).HasForeignKey(x=>x.EventId).OnDelete(DeleteBehavior.NoAction); b.HasOne(x=>x.Participant).WithMany(x=>x.Enrolments).HasForeignKey(x=>x.ParticipantId).OnDelete(DeleteBehavior.NoAction); b.HasOne(x=>x.Category).WithMany(x=>x.Enrolments).HasForeignKey(x=>x.CategoryId).OnDelete(DeleteBehavior.NoAction); });
        model.Entity<RaceResult>(b => { b.ToTable("Results"); b.HasKey(x=>x.Id); b.HasIndex(x=>x.EnrolmentId).IsUnique(); b.HasOne(x=>x.Enrolment).WithOne(x=>x.Result).HasForeignKey<RaceResult>(x=>x.EnrolmentId).OnDelete(DeleteBehavior.Cascade); });
    }
}

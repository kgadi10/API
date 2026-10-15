using Microsoft.EntityFrameworkCore;
using RaceDay.Models;

namespace RaceDay.Data;

public class RaceDayDbContext : DbContext
{
    public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<RaceEvent> Events => Set<RaceEvent>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<RaceRoute> Routes => Set<RaceRoute>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<Result> Results => Set<Result>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email).IsUnique();

        modelBuilder.Entity<User>()
            .Property(u => u.Email).HasMaxLength(256);

        modelBuilder.Entity<User>()
            .Property(u => u.Role).HasMaxLength(20);

        modelBuilder.Entity<RaceEvent>()
            .HasOne(e => e.Organiser)
            .WithMany(u => u.OrganisedEvents)
            .HasForeignKey(e => e.OrganiserID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RaceEvent>()
            .HasOne(e => e.Route)
            .WithOne(r => r.Event)
            .HasForeignKey<RaceRoute>(r => r.EventID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Category>()
            .HasOne(c => c.Event)
            .WithMany(e => e.Categories)
            .HasForeignKey(c => c.EventID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Enrolment>()
            .HasOne(e => e.Participant)
            .WithMany(u => u.Enrolments)
            .HasForeignKey(e => e.ParticipantID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Enrolment>()
            .HasOne(e => e.Event)
            .WithMany(e => e.Enrolments)
            .HasForeignKey(e => e.EventID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Enrolment>()
            .HasOne(e => e.Category)
            .WithMany(c => c.Enrolments)
            .HasForeignKey(e => e.CategoryID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Enrolment>()
            .HasIndex(e => new { e.ParticipantID, e.EventID, e.CategoryID })
            .IsUnique();

        modelBuilder.Entity<Result>()
            .HasOne(r => r.Enrolment)
            .WithOne(e => e.Result)
            .HasForeignKey<Result>(r => r.EnrolmentID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Result>()
            .HasIndex(r => r.EnrolmentID).IsUnique();

        modelBuilder.Entity<Category>()
            .Property(c => c.EntryFee).HasPrecision(10, 2);

        modelBuilder.Entity<Category>()
            .Property(c => c.DistanceKM).HasPrecision(8, 2);

        modelBuilder.Entity<RaceRoute>()
            .Property(r => r.DistanceKM).HasPrecision(8, 2);
    }
}
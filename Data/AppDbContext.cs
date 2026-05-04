using Microsoft.EntityFrameworkCore;
using RacingTelemetry.Models;

namespace RacingTelemetry.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DriverSession>()
            .HasKey(ds => new { ds.DriverId, ds.SessionId });
    }

    public DbSet<Driver> Drivers { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<Lap> Laps { get; set; }
    public DbSet<DriverSession> DriverSessions { get; set; }
}
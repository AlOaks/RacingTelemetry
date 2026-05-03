using Microsoft.EntityFrameworkCore;
using RacingTelemetry.Models;

namespace RacingTelemetry.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Driver> Drivers { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<Lap> Laps { get; set; }
}
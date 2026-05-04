using RacingTelemetry.Models;
using RacingTelemetry.Data;
using RacingTelemetry.Validators;
using Microsoft.EntityFrameworkCore;

namespace RacingTelemetry.Routes;

public static class SessionRoutes
{
    public static void MapSessionRoutes(this WebApplication app)
    {
        app.MapPost("/sessions", async (AppDbContext db, Session session) =>
        {
            var validator = new SessionValidator();
            var validationResult = validator.Validate(session);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            session.Id = 0;
            db.Sessions.Add(session);
            await db.SaveChangesAsync();
            return Results.Created($"/sessions/{session.Id}", session);

        });

        app.MapGet("/sessions", async (AppDbContext db) => await db.Sessions.ToListAsync());

        app.MapGet("/sessions/{id}", async (AppDbContext db, int id) =>
        {
            var session = await db.Sessions.FindAsync(id);

            return session is null ? Results.NotFound() : Results.Ok(session);
        });

        app.MapGet("/sessions/{id}/laps", async (AppDbContext db, int id) =>
        {
            var laps = await db.Laps.Where(l => l.SessionId == id).OrderBy(l => l.LapNumber).ToListAsync();

            if (laps.Count == 0)
            {
                return Results.NotFound($"No laps found for session {id}");
            }

            return Results.Ok(laps);

        });

        app.MapGet("/sessions/{id}/leaderboard", async (AppDbContext db, MongoDbContext mdb, int id) =>
        {
            var session = db.Sessions.FirstOrDefault(s => s.Id == id);
            if (session is null)
            {
                return Results.NotFound("No session found");
            }

            var sessionDrivers = await db.DriverSessions.Where(ds => ds.SessionId == id).ToListAsync();

            var driversIds = sessionDrivers.Select(sd => sd.DriverId).ToList();
            var drivers = await db.Drivers.Where(d => driversIds.Contains(d.Id)).ToListAsync();

            var fastestLaps = await db.Laps
                .Where(l => l.SessionId == id && driversIds.Contains(l.DriverId))
                .GroupBy(l => l.DriverId)
                .Select(g => g.OrderBy(l => l.LapTime).First())
                .ToListAsync();

            var leaderboard = fastestLaps.OrderBy(l => l.LapTime).Select(l =>
            {
                var driver = drivers.FirstOrDefault(d => d.Id == l.DriverId);
                return new
                {
                    DriverName = driver?.Name,
                    Team = driver?.Team,
                    DriverNumber = driver?.DriverNumber,
                    DriverId = driver?.Id,
                    l.LapNumber,
                    l.LapTime,
                    l.Sector1Time,
                    l.Sector2Time,
                    l.Sector3Time
                };
            }).ToList();

            return Results.Ok(leaderboard);

        });
    }
}
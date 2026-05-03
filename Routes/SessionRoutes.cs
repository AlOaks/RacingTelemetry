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

            var driver = await db.Drivers.FindAsync(session.DriverId);
            if (driver is null)
            {
                return Results.NotFound("Driver not found");
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
    }
}
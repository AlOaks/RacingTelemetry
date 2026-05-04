using RacingTelemetry.Models;
using RacingTelemetry.Data;
using RacingTelemetry.Validators;
using Microsoft.EntityFrameworkCore;

namespace RacingTelemetry.Routes;

public static class DriverRoutes
{
    public static void MapDriverRoutes(this WebApplication app)
    {
        app.MapPost("/drivers", async (AppDbContext db, Driver driver) =>
        {
            var validator = new DriverValidator();
            var validationResult = validator.Validate(driver);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            driver.Id = 0;
            db.Drivers.Add(driver);
            await db.SaveChangesAsync();
            return Results.Created($"/drivers/{driver.Id}", driver);
        });


        app.MapGet("/drivers", async (AppDbContext db) => await db.Drivers.ToListAsync());

        app.MapGet("/drivers/{id}", async (AppDbContext db, int id) =>
        {
            var driver = await db.Drivers.FindAsync(id);

            return driver is null ? Results.NotFound() : Results.Ok(driver);
        });

        app.MapGet("/drivers/{id}/sessions", async (AppDbContext db, int id) =>
        {
            var driverSessions = await db.DriverSessions.Where(ds => ds.DriverId == id).ToListAsync();

            if (driverSessions.Count == 0) return Results.NotFound($"No sessions found for driver {id}");

            // Get only the Ids of the driverSessions
            var sessionIds = driverSessions.Select(ds => ds.SessionId).ToList();
            // Fetch by checking if session in scope is withing the sessionsIds of the DriverSessions
            var sessions = await db.Sessions.Where(s => sessionIds.Contains(s.Id)).ToListAsync();

            return sessions.Count == 0 ? Results.NotFound($"No sessions for driver {id}") : Results.Ok(sessions);

        });
    }
}
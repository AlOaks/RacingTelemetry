using RacingTelemetry.Data;
using RacingTelemetry.Validators;
using RacingTelemetry.DTOs;
using RacingTelemetry.Models;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
namespace RacingTelemetry.Routes;

public static class LapRoutes
{
    public static void MapLapRoutes(this WebApplication app)
    {
        app.MapGet("/laps/session/{sessionId}/fastest", async (AppDbContext db, int sessionId) =>
        {
            var fastestLap = await db.Laps.Where(l => l.SessionId == sessionId).OrderBy(l => l.LapTime).FirstOrDefaultAsync();

            if (fastestLap is null)
            {
                return Results.NotFound($"No laps found for session {sessionId}");
            }

            return Results.Ok(fastestLap);
        });

        app.MapGet("/laps/{id}/telemetry", async (MongoDbContext mdb, int id) =>
        {
            var points = await mdb.TelemetryPoints.Find(tp => tp.LapId == id).SortByDescending(tp => tp.Timestamp).ToListAsync();


            if (points.Count == 0)
            {
                return Results.NotFound($"No telemetry found for lap {id}");
            }

            return Results.Ok(points);
        });

        app.MapPost("/laps", async (AppDbContext db, MongoDbContext mdb, LapSubmission lapSubmission) =>
        {
            var validator = new LapValidator();
            var validationResult = validator.Validate(lapSubmission);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            var session = await db.Sessions.FindAsync(lapSubmission.SessionId);
            if (session is null)
            {
                return Results.NotFound("Session not found");
            }

            var lap = new Lap
            {
                Id = 0,
                SessionId = lapSubmission.SessionId,
                Sector1Time = lapSubmission.Sector1Time,
                Sector2Time = lapSubmission.Sector2Time,
                Sector3Time = lapSubmission.Sector3Time,
                LapNumber = lapSubmission.LapNumber,
                LapTime = lapSubmission.LapTime,
                IsPitOutLap = lapSubmission.IsPitOutLap,
            };

            var newLap = db.Laps.Add(lap);
            await db.SaveChangesAsync();

            var telemetryPoints = new List<TelemetryPoint>();
            foreach (var telemetryPoint in lapSubmission.TelemetryPoints)
            {
                telemetryPoint.LapId = newLap.Entity.Id;
                telemetryPoints.Add(telemetryPoint);
            }
            await mdb.TelemetryPoints.InsertManyAsync(telemetryPoints);


            return Results.Created($"/laps/{lap.Id}", lap);
        });
    }
}
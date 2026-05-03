using System.Text.Json;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.SignalR;

using RacingTelemetry.Services;
using RacingTelemetry.Models;
using RacingTelemetry.Data;
using RacingTelemetry.Hubs;

namespace RacingTelemetry.Routes;

public static class SyncRoutes
{
    public static void MapSyncRoutes(this WebApplication app)
    {
        app.MapPost("/sync/session/{sessionKey}/{driverNumber}",
            async (int sessionKey, int driverNumber, AppDbContext db, MongoDbContext mdb, OpenF1Service openF1, IMemoryCache cache, IHubContext<LeaderboardHub> hubContext) =>
        {

            // Get Session and Driver Data
            var sessionData = await openF1.GetSessionAsync(sessionKey);
            var driverData = await openF1.GetDriverAsync(driverNumber, sessionKey);

            var sessionJson = sessionData[0];
            var driverJson = driverData[0];

            // Get existing driver, if not create a new one.
            var existingDriver = db.Drivers.FirstOrDefault(d => d.DriverNumber == driverNumber);

            var driver = existingDriver ?? new Driver
            {
                Id = 0,
                Name = driverJson.GetProperty("full_name").GetString()!,
                DriverNumber = driverJson.GetProperty("driver_number").GetInt32(),
                Headshot = driverJson.GetProperty("headshot_url").GetString(),
                Team = driverJson.GetProperty("team_name").GetString()!
            };

            // If it wasn't a existing one, save it to DB.
            if (existingDriver is null)
            {
                db.Drivers.Add(driver);
                await db.SaveChangesAsync();
            }

            var existingSession = db.Sessions.FirstOrDefault(s => s.DriverId == driver.Id && s.SessionKey == sessionKey);
            // Let's create the session now
            var session = existingSession ?? new Session
            {
                Id = 0,
                Circuit = sessionJson.GetProperty("circuit_short_name").GetString()!,
                Country = sessionJson.GetProperty("country_name").GetString()!,
                Date = sessionJson.GetProperty("date_start").GetDateTime().ToUniversalTime()!,
                DriverId = driver.Id,
                SessionName = sessionJson.GetProperty("session_name").GetString()!,
                SessionKey = sessionKey,
            };
            if (existingSession is null)
            {
                db.Sessions.Add(session);
                await db.SaveChangesAsync();
            }


            // Let's fetch the laps for this session and this driver
            var lapsData = await openF1.GetLapsAsync(sessionKey, driverNumber);
            var lapsList = lapsData.EnumerateArray().ToList();

            // Let's fetch the car data for the session
            var carData = await openF1.GetCarDataAsync(sessionKey, driverNumber, session.Date);
            var carDataList = carData.EnumerateArray().ToList();

            // Now, let's process each lap
            foreach (var lapJson in lapsList)
            {
                if (lapJson.GetProperty("lap_duration").ValueKind == JsonValueKind.Null) continue;

                var lap = new Lap
                {
                    Id = 0,
                    SessionId = session.Id,
                    Sector1Time = lapJson.GetProperty("duration_sector_1").ValueKind != JsonValueKind.Null
                        ? lapJson.GetProperty("duration_sector_1").GetSingle() : 0,
                    Sector2Time = lapJson.GetProperty("duration_sector_2").ValueKind != JsonValueKind.Null
                        ? lapJson.GetProperty("duration_sector_2").GetSingle() : 0,
                    Sector3Time = lapJson.GetProperty("duration_sector_3").ValueKind != JsonValueKind.Null
                        ? lapJson.GetProperty("duration_sector_3").GetSingle() : 0,
                    LapTime = lapJson.GetProperty("lap_duration").GetSingle(),
                    IsPitOutLap = lapJson.GetProperty("is_pit_out_lap").GetBoolean(),
                    LapNumber = lapJson.GetProperty("lap_number").GetInt32(),
                };

                db.Laps.Add(lap);
                await db.SaveChangesAsync();

                // Let's process the telemetry points.
                var telemetryPoints = carDataList.Take(100).Select(point => new TelemetryPoint
                {
                    LapId = lap.Id,
                    Gear = point.GetProperty("n_gear").GetInt32(),
                    RPM = point.GetProperty("rpm").GetInt32(),
                    Speed = point.GetProperty("speed").GetInt32(),
                    ThrottlePercentage = point.GetProperty("throttle").GetInt32(),
                    DRS = point.GetProperty("drs").GetInt32(),
                    BrakePercentage = point.GetProperty("brake").GetInt32(),
                    Timestamp = point.GetProperty("date").GetDateTime().ToUniversalTime(),
                }).ToList();

                if (telemetryPoints.Count > 0)
                {
                    await mdb.TelemetryPoints.InsertManyAsync(telemetryPoints);
                }
            }

            cache.Remove(Constants.LeaderboardCacheKey);
            await hubContext.Clients.All.SendAsync("LeaderboardUpdated", new { message = $"Session {sessionKey} synced, leaderboard updated" });

            return Results.Ok(new { message = $"Synced Session {sessionKey} for driver {driverNumber}" });

        });
    }
}
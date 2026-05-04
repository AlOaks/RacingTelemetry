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
            async (int sessionKey, int driverNumber, AppDbContext db, MongoDbContext mdb, OpenF1Service openF1, IHubContext<SessionHub> hubContext) =>
        {
            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                // Get Session and Driver Data
                var sessionData = await openF1.GetSessionAsync(sessionKey);
                var driverData = await openF1.GetDriverAsync(driverNumber, sessionKey);

                var sessionJson = sessionData[0];
                var driverJson = driverData[0];

                // Get existing driver, if not create a new one.
                var driver = await GetOrCreateDriverAsync(db, driverNumber, driverJson);

                // Get existing session, if not create a new one.
                var session = await GetOrCreateSessionAsync(db, sessionKey, sessionJson);

                // Create a new driver session, if one already exists... 
                // this function should throw an error.
                var driverSession = await CreateDriverSessionAsync(db, driver.Id, session.Id);

                // Let's fetch the laps for this session and this driver
                var lapsList = await GetDriverLapsForSessionAsync(openF1, driverNumber, sessionKey);

                // Let's fetch the telemetry for the car for the session
                var carDataList = await GetCardDataAsync(openF1, sessionKey, driverNumber, session.Date);

                // Process laps and car data
                await ProcessLapsAndCarData(db, mdb, lapsList, carDataList, driver, session);

                // Commit the transaction
                await transaction.CommitAsync();

                // Send new message to clients
                await hubContext.Clients.All.SendAsync("SessionSynced", new { sessionId = session.Id, circuit = session.Circuit, country = session.Country });

                return Results.Ok(new { message = $"Synced Session {sessionKey} for driver {driverNumber}" });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }
    private static async Task ProcessLapsAndCarData(AppDbContext db, MongoDbContext mdb, List<JsonElement> lapsList, List<JsonElement> carDataList, Driver driver, Session session)
    {
        // Now, let's process each lap
        foreach (var lapJson in lapsList)
        {
            if (lapJson.GetProperty("lap_duration").ValueKind == JsonValueKind.Null) continue;

            var lap = new Lap
            {
                Id = 0,
                DriverId = driver.Id,
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
            var telemetryPoints = carDataList.Select(point => new TelemetryPoint
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
    }

    private static async Task<List<JsonElement>> GetCardDataAsync(OpenF1Service openF1, int sessionKey, int driverNumber, DateTime sessionDate)
    {
        var carData = await openF1.GetCarDataAsync(sessionKey, driverNumber, sessionDate);
        var carDataList = carData.EnumerateArray().ToList();

        return carDataList;
    }

    private static async Task<List<JsonElement>> GetDriverLapsForSessionAsync(OpenF1Service openF1, int driverNumber, int sessionKey)
    {
        var lapsData = await openF1.GetLapsAsync(sessionKey, driverNumber);
        var lapsList = lapsData.EnumerateArray().ToList();

        return lapsList;
    }

    private static async Task<DriverSession> CreateDriverSessionAsync(AppDbContext db, int driverId, int sessionId)
    {
        var existingSession = db.DriverSessions.FirstOrDefault(ds => ds.DriverId == driverId && ds.SessionId == sessionId);

        if (existingSession is not null) throw new InvalidOperationException($"A session for driver {driverId} in session {sessionId} already exists");

        var driverSession = new DriverSession
        {
            DriverId = driverId,
            SessionId = sessionId
        };

        db.DriverSessions.Add(driverSession);
        await db.SaveChangesAsync();
        return driverSession;
    }

    private static async Task<Session> GetOrCreateSessionAsync(AppDbContext db, int sessionKey, JsonElement sessionJson)
    {
        var session = db.Sessions.FirstOrDefault(s => s.SessionKey == sessionKey);
        if (session is not null) return session;

        var newSession = new Session
        {
            Id = 0,
            SessionKey = sessionJson.GetProperty("session_key").GetInt32(),
            SessionName = sessionJson.GetProperty("session_name").GetString()!,
            Circuit = sessionJson.GetProperty("circuit_short_name").GetString()!,
            Country = sessionJson.GetProperty("country_name").GetString()!,
            Date = sessionJson.GetProperty("date_start").GetDateTime().ToUniversalTime()!
        };

        db.Sessions.Add(newSession);
        await db.SaveChangesAsync();
        return newSession;
    }

    private static async Task<Driver> GetOrCreateDriverAsync(AppDbContext db, int driverNumber, JsonElement driverJson)
    {
        var driver = db.Drivers.FirstOrDefault(d => d.DriverNumber == driverNumber);
        if (driver is not null) return driver;

        var newDriver = new Driver
        {
            Id = 0,
            Name = driverJson.GetProperty("full_name").GetString()!,
            DriverNumber = driverJson.GetProperty("driver_number").GetInt32(),
            Headshot = driverJson.GetProperty("headshot_url").GetString(),
            Team = driverJson.GetProperty("team_name").GetString()!,
        };

        db.Drivers.Add(newDriver);
        await db.SaveChangesAsync();
        return newDriver;
    }
}
using RacingTelemetry;
using RacingTelemetry.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace RacingTelemetry.Routes;

public static class LeaderboardRoutes
{

    public static void MapLeaderboardRoutes(this WebApplication app)
    {
        app.MapGet("/leaderboard", async (AppDbContext db, IMemoryCache cache) =>
        {
            // Leaving empty to create cache later.
            if (cache.TryGetValue(Constants.LeaderboardCacheKey, out var cached)) return Results.Ok(cached);


            var leaderboard = (await db.Laps
                // Join Laps to Sessions
                .Join(db.Sessions,
                    lap => lap.SessionId,        // foreign key on Lap
                    session => session.Id,        // primary key on Session
                    (lap, session) => new { lap, session })
                // Join the result to Drivers
                .Join(db.Drivers,
                    combined => combined.session.DriverId,  // foreign key on Session
                    driver => driver.Id,                     // primary key on Driver
                    (combined, driver) => new
                    {
                        DriverName = driver.Name,
                        Team = driver.Team,
                        Circuit = combined.session.Circuit,
                        LapTime = combined.lap.LapTime,
                        LapNumber = combined.lap.LapNumber
                    })
                .ToListAsync())
                // Get fastest lap per driver
                .GroupBy(x => x.DriverName)
                .Select(g => g.OrderBy(x => x.LapTime).First())
                // Rank by fastest lap time
                .OrderBy(x => x.LapTime)
                .ToList();

            cache.Set(Constants.LeaderboardCacheKey, leaderboard, TimeSpan.FromHours(1));

            return Results.Ok(leaderboard);
        });
    }
}
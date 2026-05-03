using RacingTelemetry.Models;
using RacingTelemetry.Services;

namespace RacingTelemetry.Routes;

public static class AuthRoutes
{
    public static void MapAuthRoutes(this WebApplication app)
    {
        app.MapPost("/auth/login", (TokenService jwt, User user) =>
        {

            if (user.Username != "admin" || user.Password != "password")
            {
                return Results.Unauthorized();
            }

            return Results.Ok(jwt.GenerateToken(user.Username));
        });
    }
}
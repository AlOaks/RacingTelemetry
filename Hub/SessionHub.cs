using Microsoft.AspNetCore.SignalR;

namespace RacingTelemetry.Hubs;

public class SessionHub : Hub
{
    // Clients connect to this hub at /hubs/leaderboard
    // No methods needed here for now — we're only pushing from server to client
    // If you wanted clients to request data, you'd add methods here
}
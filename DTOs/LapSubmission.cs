using RacingTelemetry.Models;

namespace RacingTelemetry.DTOs;

public class LapSubmission
{
    public int SessionId { get; set; }
    public float Sector1Time { get; set; }
    public float Sector2Time { get; set; }
    public float Sector3Time { get; set; }
    public int LapNumber { get; set; }
    public float LapTime { get; set; }
    public Boolean IsPitOutLap { get; set; }

    public List<TelemetryPoint> TelemetryPoints { get; set; } = new List<TelemetryPoint>();
}
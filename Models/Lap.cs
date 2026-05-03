using System.ComponentModel.DataAnnotations;

namespace RacingTelemetry.Models;

public class Lap
{
    public int Id { get; set; }

    public int SessionId { get; set; }

    public float Sector1Time { get; set; }

    public float Sector2Time { get; set; }

    public float Sector3Time { get; set; }

    public int LapNumber { get; set; }

    public float LapTime { get; set; }

    public Boolean IsPitOutLap { get; set; }
}
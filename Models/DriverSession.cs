using System.ComponentModel.DataAnnotations;

namespace RacingTelemetry.Models;

public class DriverSession
{
    [Required]
    public int DriverId { get; set; }

    [Required]
    public int SessionId { get; set; }
}
using System.ComponentModel.DataAnnotations;

namespace RacingTelemetry.Models;

public class Session
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Circuit { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Country { get; set; } = string.Empty;

    [Required]
    public DateTime Date { get; set; }

    [Required]
    public int DriverId { get; set; }

    [Required]
    public Driver? Driver { get; set; } = null;

    [Required]
    public string SessionName { get; set; } = string.Empty;

    [Required]
    public int SessionKey { get; set; }
}
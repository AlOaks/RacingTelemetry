using System.ComponentModel.DataAnnotations;

namespace RacingTelemetry.Models;

public class Driver
{
    public int Id { get; set; }

    [Required]
    [MinLength(1)]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Range(1, 200)]
    public int DriverNumber { get; set; }

    [MaxLength(200)]
    public string? Headshot { get; set; }

    [Required]
    [MinLength(1)]
    [MaxLength(100)]
    public string Team { get; set; } = string.Empty;
}
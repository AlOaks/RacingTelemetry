using FluentValidation;
using RacingTelemetry.DTOs;

namespace RacingTelemetry.Validators;

public class LapValidator : AbstractValidator<LapSubmission>
{
    public LapValidator()
    {
        RuleFor(l => l.SessionId)
              .GreaterThan(0).WithMessage("Session ID must be greater than 0");

        RuleFor(l => l.Sector1Time)
              .GreaterThan(0).WithMessage("Sector 1 time must be greater than 0");

        RuleFor(l => l.Sector2Time)
              .GreaterThan(0).WithMessage("Sector 2 time must be greater than 0");

        RuleFor(l => l.Sector3Time)
              .GreaterThan(0).WithMessage("Sector 3 time must be greater than 0");

        RuleFor(l => l.LapNumber)
              .GreaterThan(0).WithMessage("Lap number must be greater than 0");

        RuleFor(l => l.LapTime)
              .GreaterThan(0).WithMessage("Lap time must be greater than 0");

        RuleFor(l => l.TelemetryPoints)
              .NotEmpty().WithMessage("Telemetry points are required");
    }
}
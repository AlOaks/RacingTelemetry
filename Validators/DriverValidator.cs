using FluentValidation;
using RacingTelemetry.Models;

namespace RacingTelemetry.Validators;

public class DriverValidator : AbstractValidator<Driver>
{
    public DriverValidator()
    {
        RuleFor(d => d.Name)
              .NotEmpty().WithMessage("Name is required")
              .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

        RuleFor(d => d.DriverNumber)
            .GreaterThan(0).WithMessage("Driver number must be greater than 0");

        RuleFor(d => d.Team).NotEmpty().WithMessage("Team is required")
        .MaximumLength(100).WithMessage("Team cannot exceed 100 characters");
    }
}
using FluentValidation;
using RacingTelemetry.Models;

namespace RacingTelemetry.Validators;

public class SessionValidator : AbstractValidator<Session>
{
    public SessionValidator()
    {
        RuleFor(s => s.Circuit)
              .NotEmpty().WithMessage("Circuit is required")
              .MaximumLength(200).WithMessage("Circuit cannot exceed 200 characters");

        RuleFor(s => s.Country)
            .NotEmpty().WithMessage("Country is required")
            .MaximumLength(100).WithMessage("Country cannot exceed 100 characters");

        RuleFor(s => s.Date)
            .NotEmpty().WithMessage("Date is required")
            .Must(date => date <= DateTime.Now).WithMessage("Date must be in the past");
    }
}
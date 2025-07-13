using FluentValidation;

namespace Jude.Server.Domains.Fraud;

public class CreateFraudIndicatorRequestValidator : AbstractValidator<CreateFraudIndicatorRequest>
{
    public CreateFraudIndicatorRequestValidator()
    {
        RuleFor(f => f.Name)
            .NotNull()
            .NotEmpty()
            .WithMessage("Fraud indicator name must not be empty")
            .MinimumLength(3)
            .WithMessage("Fraud indicator name must be at least 3 characters")
            .MaximumLength(100)
            .WithMessage("Fraud indicator name must not exceed 100 characters");

        RuleFor(f => f.Description)
            .NotNull()
            .NotEmpty()
            .WithMessage("Fraud indicator description must not be empty")
            .MaximumLength(500)
            .WithMessage("Fraud indicator description must not exceed 500 characters");
    }
}
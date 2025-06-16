namespace Jude.Server.Domains.Rules;

using FluentValidation;

public class CreateRuleRequestValidator : AbstractValidator<CreateRuleRequest>
{
    public CreateRuleRequestValidator()
    {
        RuleFor(r => r.Name)
            .NotNull()
            .NotEmpty()
            .WithMessage("Rule name must not be empty")
            .MinimumLength(3)
            .WithMessage("Rule name must be at least 3 characters")
            .MaximumLength(100)
            .WithMessage("Rule name must not exceed 100 characters");

        RuleFor(r => r.Description)
            .NotNull()
            .NotEmpty()
            .WithMessage("Rule description must not be empty")
            .MaximumLength(500)
            .WithMessage("Rule description must not exceed 500 characters");
    }
}
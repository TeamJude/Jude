namespace Jude.Server.Domains.Auth;

using FluentValidation;

public class RegistrationValidator : AbstractValidator<RegisterRequest>
{
    public RegistrationValidator()
    {
        RuleFor(u => u.Username)
            .NotNull()
            .NotEmpty()
            .WithMessage("Password must not be empty")
            .MinimumLength(3)
            .WithMessage("Password must be at least 3 characters");

        RuleFor(u => u.Password)
            .NotNull()
            .NotEmpty()
            .WithMessage("Password must not be empty")
            .MinimumLength(6)
            .WithMessage("Password must be at least 6 characters")
            .MaximumLength(60)
            .WithMessage("Password must not exceed 60 characters")
            .Matches(@"^(?=.*[A-Z])(?=.*[^a-zA-Z0-9\s]).+$")
            .WithMessage(
                "Password must have at least an uppercase and special character excluding spaces"
            );

        RuleFor(u => u.Email)
            .NotNull()
            .NotEmpty()
            .WithMessage("Email must not be empty")
            .EmailAddress()
            .WithMessage("Email must be a valid email addresss");
    }
}

public class LoginValidator : AbstractValidator<LoginRequest>
{
    public LoginValidator()
    {
        RuleFor(u => u.UserIdentifier)
            .NotNull()
            .NotEmpty()
            .WithMessage("Username or email must not be empty");

        RuleFor(u => u.Password).NotNull().NotEmpty().WithMessage("Password must not be empty");
    }
}

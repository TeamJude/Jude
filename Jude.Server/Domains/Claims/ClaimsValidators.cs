using FluentValidation;

namespace Jude.Server.Domains.Claims;

public class CreateClaimFromCIMASRequestValidator : AbstractValidator<CreateClaimFromCIMASRequest>
{
    public CreateClaimFromCIMASRequestValidator()
    {
        RuleFor(c => c.CIMASRequest)
            .NotNull()
            .WithMessage("CIMAS request is required");

        RuleFor(c => c.CIMASRequest.ClaimHeader.ClaimNumber)
            .NotEmpty()
            .WithMessage("Claim number is required")
            .When(c => c.CIMASRequest != null);

        RuleFor(c => c.CIMASRequest.Patient.Personal.FirstName)
            .NotEmpty()
            .WithMessage("Patient first name is required")
            .When(c => c.CIMASRequest?.Patient?.Personal != null);

        RuleFor(c => c.CIMASRequest.Patient.Personal.Surname)
            .NotEmpty()
            .WithMessage("Patient surname is required")
            .When(c => c.CIMASRequest?.Patient?.Personal != null);

        RuleFor(c => c.CIMASRequest.Provider.PracticeNumber)
            .NotEmpty()
            .WithMessage("Provider practice number is required")
            .When(c => c.CIMASRequest?.Provider != null);

        RuleFor(c => c.CIMASRequest.ClaimHeader.TotalValues.GrossAmount)
            .GreaterThan(0)
            .WithMessage("Claim amount must be greater than zero")
            .When(c => c.CIMASRequest?.ClaimHeader?.TotalValues != null);
    }
}

public class ReviewClaimRequestValidator : AbstractValidator<ReviewClaimRequest>
{
    public ReviewClaimRequestValidator()
    {
        RuleFor(r => r.Decision)
            .IsInEnum()
            .WithMessage("Invalid claim decision");

        RuleFor(r => r.ReviewerComments)
            .MaximumLength(2000)
            .WithMessage("Reviewer comments must not exceed 2000 characters")
            .When(r => !string.IsNullOrEmpty(r.ReviewerComments));

        RuleFor(r => r.RejectionReason)
            .NotEmpty()
            .WithMessage("Rejection reason is required when rejecting a claim")
            .MaximumLength(500)
            .WithMessage("Rejection reason must not exceed 500 characters")
            .When(r => r.Decision == Data.Models.ClaimDecision.Reject);
    }
}

public class ProcessClaimRequestValidator : AbstractValidator<ProcessClaimRequest>
{
    public ProcessClaimRequestValidator()
    {
        RuleFor(r => r.AgentRecommendation)
            .NotEmpty()
            .WithMessage("Agent recommendation is required")
            .MaximumLength(100)
            .WithMessage("Agent recommendation must not exceed 100 characters");

        RuleFor(r => r.AgentReasoning)
            .NotEmpty()
            .WithMessage("Agent reasoning is required")
            .MaximumLength(2000)
            .WithMessage("Agent reasoning must not exceed 2000 characters");

        RuleFor(r => r.AgentConfidenceScore)
            .InclusiveBetween(0m, 1m)
            .WithMessage("Agent confidence score must be between 0 and 1");
    }
}
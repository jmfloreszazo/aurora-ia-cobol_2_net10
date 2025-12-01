using CardDemo.Application.Common.DTOs;
using FluentValidation;

namespace CardDemo.Application.Common.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required")
            .MaximumLength(8).WithMessage("User ID must not exceed 8 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters");
    }
}

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required")
            .MaximumLength(8).WithMessage("User ID must not exceed 8 characters")
            .Matches("^[a-zA-Z0-9]+$").WithMessage("User ID must contain only letters and numbers");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one number");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Password confirmation is required")
            .Equal(x => x.Password).WithMessage("Passwords must match");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(25).WithMessage("First name must not exceed 25 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(25).WithMessage("Last name must not exceed 25 characters");

        RuleFor(x => x.UserType)
            .NotEmpty().WithMessage("User type is required")
            .Must(x => x == "USER" || x == "ADMIN").WithMessage("User type must be USER or ADMIN");
    }
}

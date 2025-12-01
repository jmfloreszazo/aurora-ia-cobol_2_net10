using CardDemo.Application.Common.DTOs;
using FluentValidation;

namespace CardDemo.Application.Common.Validators;

public class CreateAccountRequestValidator : AbstractValidator<CreateAccountRequest>
{
    public CreateAccountRequestValidator()
    {
        RuleFor(x => x.CustomerId)
            .GreaterThan(0).WithMessage("Customer ID must be greater than 0");

        RuleFor(x => x.CreditLimit)
            .GreaterThan(0).WithMessage("Credit limit must be greater than 0")
            .LessThanOrEqualTo(1000000).WithMessage("Credit limit must not exceed $1,000,000");

        RuleFor(x => x.CashCreditLimit)
            .GreaterThanOrEqualTo(0).WithMessage("Cash credit limit must be non-negative")
            .LessThanOrEqualTo(x => x.CreditLimit).WithMessage("Cash credit limit cannot exceed credit limit");

        RuleFor(x => x.OpenDate)
            .NotEmpty().WithMessage("Open date is required")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Open date cannot be in the future");

        RuleFor(x => x.ExpirationDate)
            .NotEmpty().WithMessage("Expiration date is required")
            .GreaterThan(x => x.OpenDate).WithMessage("Expiration date must be after open date");

        RuleFor(x => x.ZipCode)
            .NotEmpty().WithMessage("Zip code is required")
            .MaximumLength(10).WithMessage("Zip code must not exceed 10 characters");
    }
}

public class UpdateAccountRequestValidator : AbstractValidator<UpdateAccountRequest>
{
    public UpdateAccountRequestValidator()
    {
        When(x => x.CreditLimit.HasValue, () =>
        {
            RuleFor(x => x.CreditLimit!.Value)
                .GreaterThan(0).WithMessage("Credit limit must be greater than 0")
                .LessThanOrEqualTo(1000000).WithMessage("Credit limit must not exceed $1,000,000");
        });

        When(x => x.CashCreditLimit.HasValue, () =>
        {
            RuleFor(x => x.CashCreditLimit!.Value)
                .GreaterThanOrEqualTo(0).WithMessage("Cash credit limit must be non-negative");
        });

        When(x => x.ActiveStatus != null, () =>
        {
            RuleFor(x => x.ActiveStatus!)
                .Must(x => x == "Y" || x == "N").WithMessage("Active status must be Y or N");
        });
    }
}

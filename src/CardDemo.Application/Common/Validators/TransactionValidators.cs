using CardDemo.Application.Common.DTOs;
using FluentValidation;

namespace CardDemo.Application.Common.Validators;

public class CreateTransactionRequestValidator : AbstractValidator<CreateTransactionRequest>
{
    public CreateTransactionRequestValidator()
    {
        RuleFor(x => x.AccountId)
            .GreaterThan(0).WithMessage("Account ID must be greater than 0");

        RuleFor(x => x.CardNumber)
            .NotEmpty().WithMessage("Card number is required")
            .Length(16).WithMessage("Card number must be exactly 16 characters")
            .Matches("^[0-9]+$").WithMessage("Card number must contain only digits");

        RuleFor(x => x.TransactionType)
            .NotEmpty().WithMessage("Transaction type is required")
            .MaximumLength(2).WithMessage("Transaction type must be 2 characters");

        RuleFor(x => x.CategoryCode)
            .GreaterThan(0).WithMessage("Category code must be greater than 0");

        RuleFor(x => x.TransactionSource)
            .NotEmpty().WithMessage("Transaction source is required")
            .MaximumLength(10).WithMessage("Transaction source must not exceed 10 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(100).WithMessage("Description must not exceed 100 characters");

        RuleFor(x => x.Amount)
            .NotEqual(0).WithMessage("Amount cannot be zero");

        When(x => x.MerchantId != null, () =>
        {
            RuleFor(x => x.MerchantId!)
                .MaximumLength(9).WithMessage("Merchant ID must not exceed 9 characters");
        });

        When(x => x.MerchantName != null, () =>
        {
            RuleFor(x => x.MerchantName!)
                .MaximumLength(50).WithMessage("Merchant name must not exceed 50 characters");
        });
    }
}

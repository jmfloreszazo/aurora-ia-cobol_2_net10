using CardDemo.Application.Common.DTOs;
using MediatR;

namespace CardDemo.Application.Features.Transactions.Commands;

public record CreateTransactionCommand(
    long AccountId,
    string CardNumber,
    string TransactionType,
    int CategoryCode,
    string TransactionSource,
    string Description,
    decimal Amount,
    string? MerchantId,
    string? MerchantName,
    string? MerchantCity,
    string? MerchantZip,
    DateTime? TransactionDate
) : IRequest<TransactionResponse>;

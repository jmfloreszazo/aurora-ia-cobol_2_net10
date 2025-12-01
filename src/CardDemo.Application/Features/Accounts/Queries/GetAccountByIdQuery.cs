using CardDemo.Application.Common.DTOs;
using MediatR;

namespace CardDemo.Application.Features.Accounts.Queries;

public record GetAccountByIdQuery(long AccountId) : IRequest<AccountDetailResponse>;

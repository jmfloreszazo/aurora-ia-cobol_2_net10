using CardDemo.Application.Common.DTOs;
using MediatR;

namespace CardDemo.Application.Features.Auth.Commands;

public record RegisterCommand(
    string UserId,
    string Password,
    string FirstName,
    string LastName,
    string UserType
) : IRequest<RegisterResponse>;

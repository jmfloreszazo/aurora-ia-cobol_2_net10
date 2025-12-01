using CardDemo.Application.Common.DTOs;
using MediatR;

namespace CardDemo.Application.Features.Auth.Commands;

public record LoginCommand(string UserId, string Password) : IRequest<LoginResponse>;

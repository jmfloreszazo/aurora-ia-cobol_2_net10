namespace CardDemo.Application.Common.DTOs;

public record LoginRequest(
    string UserId,
    string Password
);

public record LoginResponse(
    string Token,
    string UserId,
    string FirstName,
    string LastName,
    string UserType,
    DateTime? LastLoginAt
);

public record RegisterRequest(
    string UserId,
    string Password,
    string ConfirmPassword,
    string FirstName,
    string LastName,
    string UserType
);

public record RegisterResponse(
    string UserId,
    string Message
);

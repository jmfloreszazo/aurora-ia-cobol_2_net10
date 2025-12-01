using CardDemo.Domain.Entities;

namespace CardDemo.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}

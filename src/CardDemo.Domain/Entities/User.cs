using CardDemo.Domain.Common;
using CardDemo.Domain.Enums;

namespace CardDemo.Domain.Entities;

public class User : BaseEntity
{
    public string UserId { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public UserRole UserType { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsLocked { get; set; } = false;
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? LastLoginAt { get; set; }

    public string FullName => $"{FirstName} {LastName}";
}

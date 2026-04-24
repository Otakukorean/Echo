namespace Identity.Identity.Models;

public class User : Aggregate<Guid>
{
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string DisplayName { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool EmailConfirmed { get; private set; } = false;
    public SystemRole Role { get; private set; } = SystemRole.USER;
    public List<Session> Sessions { get; private set; } = new();

    private User() { }

    public static User Create(string email, string passwordHash, string displayName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        return new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHash,
            DisplayName = displayName,
        };
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void ConfirmEmail()
    {
        EmailConfirmed = true;
    }

    public void AssignRole(SystemRole role)
    {
        Role = role;
    }
}

namespace Shared.Email;

public class EmailSettings
{
    public const string SectionName = "Email";

    public string SmtpHost { get; init; } = "smtp.gmail.com";
    public int SmtpPort { get; init; } = 587;
    public string FromEmail { get; init; } = "noreply@echo.com";
    public string FromName { get; init; } = "Echo";
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

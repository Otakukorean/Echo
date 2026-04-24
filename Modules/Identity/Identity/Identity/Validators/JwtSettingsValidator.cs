using Microsoft.Extensions.Options;

namespace Identity.Identity.Validators;

public class JwtSettingsValidator : IValidateOptions<JwtSettings>
{
    public ValidateOptionsResult Validate(string? name, JwtSettings options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Secret) || options.Secret.Length < 32)
            errors.Add("Secret must be at least 32 characters.");

        if (string.IsNullOrWhiteSpace(options.Issuer))
            errors.Add("Issuer is required.");

        if (string.IsNullOrWhiteSpace(options.Audience))
            errors.Add("Audience is required.");

        if (options.AccessTokenExpirationInMinutes <= 0)
            errors.Add("AccessTokenExpirationInMinutes must be greater than 0.");

        if (options.RefreshTokenExpirationInDays <= 0)
            errors.Add("RefreshTokenExpirationInDays must be greater than 0.");

        return errors.Count > 0
            ? ValidateOptionsResult.Fail(errors)
            : ValidateOptionsResult.Success;
    }
}

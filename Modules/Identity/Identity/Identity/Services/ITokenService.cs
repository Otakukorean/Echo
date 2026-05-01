namespace Identity.Identity.Services;

public interface ITokenService
{
    string GenerateAccessToken(User user, Guid? storeId = null);
    string GenerateRefreshToken();
}

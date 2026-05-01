namespace Shared.Contracts.Identity;

public record ReissuedToken(string AccessToken, DateTime ExpiresAt);

public interface ITokenReissuer
{
    ReissuedToken GenerateAccessToken(Guid userId, string email, string role, Guid? storeId);
}

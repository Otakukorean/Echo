using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Shared.Contracts.StoreContext;
using Shared.Exceptions;

namespace Shared.StoreContext;

public class StoreContext : IStoreContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IStoreOwnershipChecker _ownershipChecker;
    private VerifiedStore? _cachedStore;
    private bool _claimsParsed;
    private Guid _userId;
    private Guid? _storeId;

    public StoreContext(IHttpContextAccessor httpContextAccessor, IStoreOwnershipChecker ownershipChecker)
    {
        _httpContextAccessor = httpContextAccessor;
        _ownershipChecker = ownershipChecker;
    }

    public Guid UserId
    {
        get
        {
            EnsureClaimsParsed();
            return _userId;
        }
    }

    public Guid? StoreId
    {
        get
        {
            EnsureClaimsParsed();
            return _storeId;
        }
    }

    public async Task<VerifiedStore> GetVerifiedStoreAsync(CancellationToken cancellationToken = default)
    {
        if (_cachedStore is not null)
            return _cachedStore;

        EnsureClaimsParsed();

        if (StoreId is null)
            throw new UnauthorizedException("No store associated with the current user");

        var store = await _ownershipChecker.GetVerifiedStoreAsync(StoreId.Value, UserId, cancellationToken);

        if (store is null)
            throw new UnauthorizedException("Store ownership verification failed");

        _cachedStore = store;
        return _cachedStore;
    }

    private void EnsureClaimsParsed()
    {
        if (_claimsParsed)
            return;

        var user = _httpContextAccessor.HttpContext?.User;

        var subClaim = user?.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? user?.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(subClaim) || !Guid.TryParse(subClaim, out var parsedUserId))
            throw new UnauthorizedException("Invalid or missing user identifier in token");

        _userId = parsedUserId;

        var storeIdClaim = user?.FindFirstValue("store_id");

        if (!string.IsNullOrWhiteSpace(storeIdClaim) && Guid.TryParse(storeIdClaim, out var parsedStoreId))
            _storeId = parsedStoreId;
        else
            _storeId = null;

        _claimsParsed = true;
    }
}

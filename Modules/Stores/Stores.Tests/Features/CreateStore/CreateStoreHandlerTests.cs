using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Stores.Data;
using Stores.Stores.Dtos;
using Stores.Stores.Exceptions;
using Stores.Stores.Features.CreateStore;
using Stores.Stores.Models;
using Stores.Tests.Helpers;
using Xunit;

namespace Stores.Tests.Features.CreateStore;

public class CreateStoreHandlerTests : IDisposable
{
    private readonly StoresDbContext _dbContext;
    private readonly CreateStoreHandler _handler;
    private readonly Guid _ownerId = Guid.NewGuid();

    public CreateStoreHandlerTests()
    {
        _dbContext = DbContextFactory.Create();
        _handler = new CreateStoreHandler(_dbContext);
    }

    private CreateStoreDto ValidDto(string slug = "my-store") =>
        new("My Store", slug, "A test store", "https://example.com/logo.png", "https://example.com/cover.png");

    [Fact]
    public async Task Handle_ValidCommand_CreatesStoreAndReturnsDto()
    {
        var command = new CreateStoreCommand(ValidDto(), _ownerId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Store.Should().NotBeNull();
        result.Store.Name.Should().Be("My Store");
        result.Store.Slug.Should().Be("my-store");
        result.Store.Description.Should().Be("A test store");
        result.Store.LogoUrl.Should().Be("https://example.com/logo.png");
        result.Store.CoverUrl.Should().Be("https://example.com/cover.png");
        result.Store.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_ValidCommand_PersistsStoreInDatabase()
    {
        var command = new CreateStoreCommand(ValidDto(), _ownerId);

        await _handler.Handle(command, CancellationToken.None);

        var store = await _dbContext.Stores.SingleOrDefaultAsync(s => s.Slug == "my-store");
        store.Should().NotBeNull();
        store!.OwnerId.Should().Be(_ownerId);
    }

    [Fact]
    public async Task Handle_DuplicateSlug_ThrowsStoreAlreadyExist()
    {
        var existingStore = Store.Create("Existing", "taken-slug", "desc", "https://logo.png", null, Guid.NewGuid());
        _dbContext.Stores.Add(existingStore);
        await _dbContext.SaveChangesAsync();

        var command = new CreateStoreCommand(ValidDto("taken-slug"), _ownerId);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<StoreAlreadyExist>()
            .WithMessage("A store with this slug already exists");
    }

    [Fact]
    public async Task Handle_UserAlreadyOwnsStore_ThrowsUserAlreadyHasStore()
    {
        var existingStore = Store.Create("First Store", "first-store", "desc", "https://logo.png", null, _ownerId);
        _dbContext.Stores.Add(existingStore);
        await _dbContext.SaveChangesAsync();

        var command = new CreateStoreCommand(ValidDto("second-store"), _ownerId);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UserAlreadyHasStore>()
            .WithMessage("User already owns a store");
    }

    [Fact]
    public async Task Handle_UserAlreadyOwnsStore_DoesNotCreateSecondStore()
    {
        var existingStore = Store.Create("First", "first", "desc", "https://logo.png", null, _ownerId);
        _dbContext.Stores.Add(existingStore);
        await _dbContext.SaveChangesAsync();

        var command = new CreateStoreCommand(ValidDto("second"), _ownerId);

        try { await _handler.Handle(command, CancellationToken.None); } catch { }

        var count = await _dbContext.Stores.CountAsync();
        count.Should().Be(1);
    }

    [Fact]
    public async Task Handle_DifferentOwners_CanCreateStoresWithDifferentSlugs()
    {
        var owner1 = Guid.NewGuid();
        var owner2 = Guid.NewGuid();

        await _handler.Handle(new CreateStoreCommand(ValidDto("store-one"), owner1), CancellationToken.None);
        await _handler.Handle(new CreateStoreCommand(ValidDto("store-two"), owner2), CancellationToken.None);

        var count = await _dbContext.Stores.CountAsync();
        count.Should().Be(2);
    }

    [Fact]
    public async Task Handle_NullCoverUrl_CreatesStoreSuccessfully()
    {
        var dto = new CreateStoreDto("No Cover", "no-cover", "desc", "https://logo.png", null);
        var command = new CreateStoreCommand(dto, _ownerId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Store.CoverUrl.Should().BeNull();
    }

    [Fact]
    public async Task Handle_OwnershipCheckRunsBeforeSlugCheck()
    {
        // Owner already has a store with slug "existing"
        var existingStore = Store.Create("Existing", "existing", "desc", "https://logo.png", null, _ownerId);
        _dbContext.Stores.Add(existingStore);
        await _dbContext.SaveChangesAsync();

        // Try to create another store with the SAME slug — should get UserAlreadyHasStore, not StoreAlreadyExist
        var command = new CreateStoreCommand(ValidDto("existing"), _ownerId);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UserAlreadyHasStore>();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}

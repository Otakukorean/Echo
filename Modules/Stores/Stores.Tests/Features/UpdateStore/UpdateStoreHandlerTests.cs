using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Stores.Data;
using Stores.Stores.Dtos;
using Stores.Stores.Exceptions;
using Stores.Stores.Features.Dashboard.UpdateStore;
using Stores.Stores.Models;
using Stores.Tests.Helpers;
using Xunit;

namespace Stores.Tests.Features.UpdateStore;

public class UpdateStoreHandlerTests : IDisposable
{
    private readonly StoresDbContext _dbContext;
    private readonly UpdateStoreHandler _handler;
    private readonly Guid _ownerId = Guid.NewGuid();

    public UpdateStoreHandlerTests()
    {
        _dbContext = DbContextFactory.Create();
        _handler = new UpdateStoreHandler(_dbContext);
    }

    private Store SeedStore(string name = "Original", string slug = "original", Guid? ownerId = null)
    {
        var store = Store.Create(name, slug, "Original description", "https://example.com/logo.png", "https://example.com/cover.png", ownerId ?? _ownerId);
        _dbContext.Stores.Add(store);
        _dbContext.SaveChanges();
        return store;
    }

    private static UpdateStoreDto ValidUpdateDto(
        string name = "Updated Store",
        string slug = "updated-store",
        string description = "Updated description",
        string logoUrl = "https://example.com/new-logo.png",
        string? coverUrl = "https://example.com/new-cover.png") =>
        new(name, slug, description, logoUrl, coverUrl);

    [Fact]
    public async Task Handle_ValidCommand_UpdatesStoreAndReturnsDto()
    {
        var store = SeedStore();
        var dto = ValidUpdateDto();
        var command = new UpdateStoreRequest(dto, store.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Store.Should().NotBeNull();
        result.Store.Id.Should().Be(store.Id);
        result.Store.Name.Should().Be("Updated Store");
        result.Store.Slug.Should().Be("updated-store");
        result.Store.Description.Should().Be("Updated description");
        result.Store.LogoUrl.Should().Be("https://example.com/new-logo.png");
        result.Store.CoverUrl.Should().Be("https://example.com/new-cover.png");
        result.Store.OwnerId.Should().Be(_ownerId);
    }

    [Fact]
    public async Task Handle_ValidCommand_PersistsChangesInDatabase()
    {
        var store = SeedStore();
        var dto = ValidUpdateDto();
        var command = new UpdateStoreRequest(dto, store.Id);

        await _handler.Handle(command, CancellationToken.None);

        var updated = await _dbContext.Stores.SingleAsync(s => s.Id == store.Id);
        updated.Name.Should().Be("Updated Store");
        updated.Slug.Should().Be("updated-store");
        updated.Description.Should().Be("Updated description");
        updated.LogoUrl.Should().Be("https://example.com/new-logo.png");
        updated.CoverUrl.Should().Be("https://example.com/new-cover.png");
    }

    [Fact]
    public async Task Handle_StoreNotFound_ThrowsStoreNotFound()
    {
        var nonExistentId = Guid.NewGuid();
        var command = new UpdateStoreRequest(ValidUpdateDto(), nonExistentId);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<StoreNotFound>()
            .WithMessage("Store not found");
    }

    [Fact]
    public async Task Handle_NullCoverUrl_UpdatesSuccessfully()
    {
        var store = SeedStore();
        var dto = ValidUpdateDto(coverUrl: null);
        var command = new UpdateStoreRequest(dto, store.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Store.CoverUrl.Should().BeNull();
    }

    [Fact]
    public async Task Handle_UpdateDoesNotAffectOtherStores()
    {
        var store1 = SeedStore("Store One", "store-one");
        var store2 = SeedStore("Store Two", "store-two", Guid.NewGuid());
        var command = new UpdateStoreRequest(ValidUpdateDto(), store1.Id);

        await _handler.Handle(command, CancellationToken.None);

        var untouched = await _dbContext.Stores.SingleAsync(s => s.Id == store2.Id);
        untouched.Name.Should().Be("Store Two");
        untouched.Slug.Should().Be("store-two");
    }

    [Fact]
    public async Task Handle_PreservesOwnerIdAfterUpdate()
    {
        var store = SeedStore();
        var command = new UpdateStoreRequest(ValidUpdateDto(), store.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Store.OwnerId.Should().Be(_ownerId);
        var persisted = await _dbContext.Stores.SingleAsync(s => s.Id == store.Id);
        persisted.OwnerId.Should().Be(_ownerId);
    }

    [Fact]
    public async Task Handle_PreservesCreatedAtAfterUpdate()
    {
        var store = SeedStore();
        var originalCreatedAt = store.CreatedAt;
        var command = new UpdateStoreRequest(ValidUpdateDto(), store.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Store.CreatedAt.Should().Be(originalCreatedAt);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}

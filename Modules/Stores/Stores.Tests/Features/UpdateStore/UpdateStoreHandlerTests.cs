using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shared.Contracts.FileStorage;
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
    private readonly IFileStorageService _fileStorage;
    private readonly UpdateStoreHandler _handler;
    private readonly Guid _ownerId = Guid.NewGuid();

    public UpdateStoreHandlerTests()
    {
        _dbContext = DbContextFactory.Create();
        _fileStorage = Substitute.For<IFileStorageService>();
        _fileStorage.UploadAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FileUploadResult("https://blob.test/new-image.png", "new-image.png"));
        _handler = new UpdateStoreHandler(_dbContext, _fileStorage);
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
        string description = "Updated description") =>
        new(name, slug, description, null, null, null, null, null, null);

    private static UpdateStoreDto ValidUpdateDtoWithImages(
        string name = "Updated Store",
        string slug = "updated-store",
        string description = "Updated description") =>
        new(name, slug, description,
            new MemoryStream([1, 2, 3]), "new-logo.png", "image/png",
            new MemoryStream([4, 5, 6]), "new-cover.png", "image/png");

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
    public async Task Handle_NoImages_KeepsExistingUrls()
    {
        var store = SeedStore();
        var dto = ValidUpdateDto();
        var command = new UpdateStoreRequest(dto, store.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Store.LogoUrl.Should().Be("https://example.com/logo.png");
        result.Store.CoverUrl.Should().Be("https://example.com/cover.png");
    }

    [Fact]
    public async Task Handle_WithImages_UploadsNewImages()
    {
        var store = SeedStore();
        var dto = ValidUpdateDtoWithImages();
        var command = new UpdateStoreRequest(dto, store.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Store.LogoUrl.Should().Be("https://blob.test/new-image.png");
        result.Store.CoverUrl.Should().Be("https://blob.test/new-image.png");
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

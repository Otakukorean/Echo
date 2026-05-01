using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shared.Contracts.FileStorage;
using Stores.Data;
using Stores.Stores.Dtos;
using Stores.Stores.Exceptions;
using Stores.Stores.Features.Dashboard.CreateStore;
using Stores.Stores.Models;
using Stores.Tests.Helpers;
using Xunit;

namespace Stores.Tests.Features.CreateStore;

public class CreateStoreHandlerTests : IDisposable
{
    private readonly StoresDbContext _dbContext;
    private readonly IFileStorageService _fileStorage;
    private readonly CreateStoreHandler _handler;
    private readonly Guid _ownerId = Guid.NewGuid();

    public CreateStoreHandlerTests()
    {
        _dbContext = DbContextFactory.Create();
        _fileStorage = Substitute.For<IFileStorageService>();
        _fileStorage.UploadAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FileUploadResult("https://blob.test/logo.png", "logo.png"));
        _handler = new CreateStoreHandler(_dbContext, _fileStorage);
    }

    private static CreateStoreDto ValidDto(string slug = "my-store") =>
        new("My Store", slug, "A test store",
            new MemoryStream([1, 2, 3]), "logo.png", "image/png",
            new MemoryStream([4, 5, 6]), "cover.png", "image/png");

    [Fact]
    public async Task Handle_ValidCommand_CreatesStoreAndReturnsDto()
    {
        var command = new CreateStoreCommand(ValidDto(), _ownerId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Store.Should().NotBeNull();
        result.Store.Name.Should().Be("My Store");
        result.Store.Slug.Should().Be("my-store");
        result.Store.Description.Should().Be("A test store");
        result.Store.OwnerId.Should().Be(_ownerId);
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
    public async Task Handle_NullCoverStream_CreatesStoreSuccessfully()
    {
        var dto = new CreateStoreDto("No Cover", "no-cover", "desc",
            new MemoryStream([1, 2, 3]), "logo.png", "image/png",
            null, null, null);
        var command = new CreateStoreCommand(dto, _ownerId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Store.CoverUrl.Should().BeNull();
    }

    [Fact]
    public async Task Handle_OwnershipCheckRunsBeforeSlugCheck()
    {
        var existingStore = Store.Create("Existing", "existing", "desc", "https://logo.png", null, _ownerId);
        _dbContext.Stores.Add(existingStore);
        await _dbContext.SaveChangesAsync();

        var command = new CreateStoreCommand(ValidDto("existing"), _ownerId);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UserAlreadyHasStore>();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}

using FluentAssertions;
using Identity.Data;
using Identity.Identity.Dtos;
using Identity.Identity.Exceptions;
using Identity.Identity.Features.CreateUser;
using Identity.Identity.Models;
using Identity.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Identity.Tests.Features.CreateUser;

public class CreateUserHandlerTests : IDisposable
{
    private readonly IdentityDbContext _dbContext;
    private readonly CreateUserHandler _handler;

    public CreateUserHandlerTests()
    {
        _dbContext = DbContextFactory.Create();
        _handler = new CreateUserHandler(_dbContext);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesUserAndReturnsSuccess()
    {
        // Arrange
        var dto = new RegisterDto("test@example.com", "Password123!", "Test User");
        var command = new CreateUserCommand(dto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Email == "test@example.com");
        user.Should().NotBeNull();
        user!.DisplayName.Should().Be("Test User");
        user.IsActive.Should().BeTrue();
        user.EmailConfirmed.Should().BeFalse();
        user.Role.Should().Be(SystemRole.USER);
    }

    [Fact]
    public async Task Handle_ValidCommand_HashesPassword()
    {
        // Arrange
        var dto = new RegisterDto("hash@example.com", "MyPlainPassword", "Hash User");
        var command = new CreateUserCommand(dto);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var user = await _dbContext.Users.SingleAsync(u => u.Email == "hash@example.com");
        user.PasswordHash.Should().NotBe("MyPlainPassword");
        BCrypt.Net.BCrypt.Verify("MyPlainPassword", user.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ThrowsUserAlreadyExist()
    {
        // Arrange
        var existingUser = User.Create("existing@example.com", BCrypt.Net.BCrypt.HashPassword("pass"), "Existing");
        _dbContext.Users.Add(existingUser);
        await _dbContext.SaveChangesAsync();

        var dto = new RegisterDto("existing@example.com", "Password123!", "New User");
        var command = new CreateUserCommand(dto);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UserAlreadyExist>()
            .WithMessage("User Already Exist");
    }

    [Fact]
    public async Task Handle_DuplicateEmail_DoesNotCreateSecondUser()
    {
        // Arrange
        var existingUser = User.Create("dup@example.com", BCrypt.Net.BCrypt.HashPassword("pass"), "First");
        _dbContext.Users.Add(existingUser);
        await _dbContext.SaveChangesAsync();

        var dto = new RegisterDto("dup@example.com", "Password123!", "Second");
        var command = new CreateUserCommand(dto);

        // Act
        try { await _handler.Handle(command, CancellationToken.None); } catch { }

        // Assert
        var count = await _dbContext.Users.CountAsync(u => u.Email == "dup@example.com");
        count.Should().Be(1);
    }

    [Fact]
    public async Task Handle_MultipleUniqueUsers_CreatesAll()
    {
        // Arrange & Act
        for (var i = 0; i < 3; i++)
        {
            var dto = new RegisterDto($"user{i}@example.com", "Password123!", $"User {i}");
            await _handler.Handle(new CreateUserCommand(dto), CancellationToken.None);
        }

        // Assert
        var count = await _dbContext.Users.CountAsync();
        count.Should().Be(3);
    }

    [Fact]
    public async Task Handle_ValidCommand_AssignsUniqueId()
    {
        // Arrange
        var dto = new RegisterDto("unique@example.com", "Password123!", "Unique");
        var command = new CreateUserCommand(dto);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var user = await _dbContext.Users.SingleAsync(u => u.Email == "unique@example.com");
        user.Id.Should().NotBe(Guid.Empty);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}

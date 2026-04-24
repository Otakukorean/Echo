using FluentAssertions;
using Identity.Data;
using Identity.Identity.Dtos;
using Identity.Identity.Exceptions;
using Identity.Identity.Features.Login;
using Identity.Identity.Models;
using Identity.Identity.Services;
using Identity.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Identity.Tests.Features.Login;

public class LoginHandlerTests : IDisposable
{
    private readonly IdentityDbContext _dbContext;
    private readonly ITokenService _tokenService;
    private readonly IOptions<JwtSettings> _jwtSettings;
    private readonly TimeProvider _timeProvider;
    private readonly LoginHandler _handler;

    public LoginHandlerTests()
    {
        _dbContext = DbContextFactory.Create();
        _tokenService = Substitute.For<ITokenService>();
        _tokenService.GenerateAccessToken(Arg.Any<User>()).Returns("fake-access-token");
        _tokenService.GenerateRefreshToken().Returns("fake-refresh-token");

        _jwtSettings = Options.Create(new JwtSettings
        {
            Secret = "TestSecretKeyThatIsAtLeast32Characters!",
            Issuer = "test-issuer",
            Audience = "test-audience",
            AccessTokenExpirationInMinutes = 15,
            RefreshTokenExpirationInDays = 7
        });

        _timeProvider = Substitute.For<TimeProvider>();
        _timeProvider.GetUtcNow().Returns(new DateTimeOffset(2025, 1, 15, 12, 0, 0, TimeSpan.Zero));

        _handler = new LoginHandler(_dbContext, _tokenService, _jwtSettings, _timeProvider);
    }

    private async Task<User> SeedUser(string email = "test@example.com", string password = "Password123!", bool isActive = true)
    {
        var hash = BCrypt.Net.BCrypt.HashPassword(password);
        var user = User.Create(email, hash, "Test User");
        if (!isActive) user.Deactivate();
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        return user;
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsTokens()
    {
        await SeedUser();
        var command = new LoginCommand(new LoginDto("test@example.com", "Password123!"), "127.0.0.1", "TestAgent");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.AccessToken.Should().Be("fake-access-token");
        result.RefreshToken.Should().Be("fake-refresh-token");
        result.ExpiresAt.Should().Be(new DateTime(2025, 1, 15, 12, 15, 0));
        result.RefreshExpiresAt.Should().Be(new DateTime(2025, 1, 22, 12, 0, 0));
    }

    [Fact]
    public async Task Handle_ValidCredentials_CreatesSession()
    {
        var user = await SeedUser();
        var command = new LoginCommand(new LoginDto("test@example.com", "Password123!"), "127.0.0.1", "TestAgent");

        await _handler.Handle(command, CancellationToken.None);

        var session = await _dbContext.Sessions.SingleOrDefaultAsync();
        session.Should().NotBeNull();
        session!.RefreshToken.Should().Be("fake-refresh-token");
        session.CreatedByIp.Should().Be("127.0.0.1");
        session.UserAgent.Should().Be("TestAgent");
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsUserNotFound()
    {
        var command = new LoginCommand(new LoginDto("nobody@example.com", "Password123!"), "127.0.0.1", "TestAgent");

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UserNotFound>().WithMessage("User not found");
    }

    [Fact]
    public async Task Handle_InactiveUser_ThrowsInvalidCredentials()
    {
        await SeedUser(isActive: false);
        var command = new LoginCommand(new LoginDto("test@example.com", "Password123!"), "127.0.0.1", "TestAgent");

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidCredentials>().WithMessage("Account is deactivated");
    }

    [Fact]
    public async Task Handle_WrongPassword_ThrowsInvalidCredentials()
    {
        await SeedUser();
        var command = new LoginCommand(new LoginDto("test@example.com", "WrongPassword"), "127.0.0.1", "TestAgent");

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidCredentials>().WithMessage("Invalid credentials");
    }

    [Fact]
    public async Task Handle_SameUserAgent_ReplacesExistingSession()
    {
        var user = await SeedUser();

        // Create an existing session with the same user agent
        var oldSession = Session.Create("old-token", DateTime.UtcNow.AddDays(7), "127.0.0.1", "TestAgent");
        _dbContext.Sessions.Add(oldSession);
        _dbContext.Entry(oldSession).Property("UserId").CurrentValue = user.Id;
        await _dbContext.SaveChangesAsync();

        var command = new LoginCommand(new LoginDto("test@example.com", "Password123!"), "127.0.0.1", "TestAgent");

        await _handler.Handle(command, CancellationToken.None);

        var sessions = await _dbContext.Sessions.ToListAsync();
        sessions.Should().HaveCount(1);
        sessions[0].RefreshToken.Should().Be("fake-refresh-token");
    }

    [Fact]
    public async Task Handle_DifferentUserAgent_KeepsBothSessions()
    {
        var user = await SeedUser();

        // Create an existing session with a different user agent
        var oldSession = Session.Create("old-token", DateTime.UtcNow.AddDays(7), "127.0.0.1", "Chrome");
        _dbContext.Sessions.Add(oldSession);
        _dbContext.Entry(oldSession).Property("UserId").CurrentValue = user.Id;
        await _dbContext.SaveChangesAsync();

        var command = new LoginCommand(new LoginDto("test@example.com", "Password123!"), "127.0.0.1", "Firefox");

        await _handler.Handle(command, CancellationToken.None);

        var sessions = await _dbContext.Sessions.ToListAsync();
        sessions.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ValidCredentials_CallsTokenService()
    {
        await SeedUser();
        var command = new LoginCommand(new LoginDto("test@example.com", "Password123!"), "127.0.0.1", "TestAgent");

        await _handler.Handle(command, CancellationToken.None);

        _tokenService.Received(1).GenerateAccessToken(Arg.Any<User>());
        _tokenService.Received(1).GenerateRefreshToken();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}

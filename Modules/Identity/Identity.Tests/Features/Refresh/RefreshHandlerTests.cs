using FluentAssertions;
using Identity.Data;
using Identity.Identity.Exceptions;
using Identity.Identity.Features.Refresh;
using Identity.Identity.Models;
using Identity.Identity.Services;
using Identity.Tests.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NSubstitute;
using Stores.Contracts.Dtos;
using Stores.Contracts.Features;
using Xunit;

namespace Identity.Tests.Features.Refresh;

public class RefreshHandlerTests : IDisposable
{
    private readonly IdentityDbContext _dbContext;
    private readonly ITokenService _tokenService;
    private readonly IOptions<JwtSettings> _jwtSettings;
    private readonly TimeProvider _timeProvider;
    private readonly ISender _sender;
    private readonly RefreshHandler _handler;

    public RefreshHandlerTests()
    {
        _dbContext = DbContextFactory.Create();
        _tokenService = Substitute.For<ITokenService>();
        _tokenService.GenerateAccessToken(Arg.Any<User>(), Arg.Any<Guid?>()).Returns("new-access-token");
        _tokenService.GenerateRefreshToken().Returns("new-refresh-token");

        _jwtSettings = Options.Create(new JwtSettings
        {
            Secret = "TestSecretKeyThatIsAtLeast32Characters!",
            Issuer = "test-issuer",
            Audience = "test-audience",
            AccessTokenExpirationInMinutes = 15,
            RefreshTokenExpirationInDays = 7
        });

        _timeProvider = Substitute.For<TimeProvider>();
        _timeProvider.GetUtcNow().Returns(new DateTimeOffset(2030, 1, 15, 12, 0, 0, TimeSpan.Zero));

        _sender = Substitute.For<ISender>();
        _sender.Send(Arg.Any<GetStoreByOwnerIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(new GetStoreByOwnerIdResult(null));

        _handler = new RefreshHandler(_dbContext, _tokenService, _jwtSettings, _timeProvider, _sender);
    }

    private async Task<(User user, Session session)> SeedUserWithActiveSession(string refreshToken = "valid-token")
    {
        var user = User.Create("user@example.com", BCrypt.Net.BCrypt.HashPassword("pass"), "Test User");
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var session = Session.Create(refreshToken, DateTime.UtcNow.AddDays(7), "127.0.0.1", "TestAgent");
        _dbContext.Sessions.Add(session);
        _dbContext.Entry(session).Property("UserId").CurrentValue = user.Id;
        await _dbContext.SaveChangesAsync();

        // Detach all entities so the handler gets a clean tracker
        _dbContext.ChangeTracker.Clear();

        return (user, session);
    }

    [Fact]
    public async Task Handle_ValidRefreshToken_ReturnsNewTokens()
    {
        await SeedUserWithActiveSession();
        var command = new RefreshCommand("valid-token", "127.0.0.1", "TestAgent");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.AccessToken.Should().Be("new-access-token");
        result.RefreshToken.Should().Be("new-refresh-token");
        result.ExpiresAt.Should().Be(new DateTime(2030, 1, 15, 12, 15, 0));
        result.RefreshExpiresAt.Should().Be(new DateTime(2030, 1, 22, 12, 0, 0));
    }

    [Fact]
    public async Task Handle_ValidRefreshToken_RevokesOldSession()
    {
        var (_, oldSession) = await SeedUserWithActiveSession();
        var command = new RefreshCommand("valid-token", "127.0.0.1", "TestAgent");

        await _handler.Handle(command, CancellationToken.None);

        var revoked = await _dbContext.Sessions.SingleAsync(s => s.Id == oldSession.Id);
        revoked.IsRevoked.Should().BeTrue();
        revoked.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ValidRefreshToken_CreatesNewSession()
    {
        await SeedUserWithActiveSession();
        var command = new RefreshCommand("valid-token", "192.168.1.1", "NewAgent");

        await _handler.Handle(command, CancellationToken.None);

        var sessions = await _dbContext.Sessions.ToListAsync();
        sessions.Should().HaveCount(2);

        var newSession = sessions.Single(s => s.RefreshToken == "new-refresh-token");
        newSession.CreatedByIp.Should().Be("192.168.1.1");
        newSession.UserAgent.Should().Be("NewAgent");
        newSession.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_UnknownToken_ThrowsInvalidSession()
    {
        await SeedUserWithActiveSession();
        var command = new RefreshCommand("unknown-token", "127.0.0.1", "TestAgent");

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidSession>().WithMessage("Invalid refresh token");
    }

    [Fact]
    public async Task Handle_ExpiredSession_ThrowsInvalidSession()
    {
        var user = User.Create("expired@example.com", BCrypt.Net.BCrypt.HashPassword("pass"), "Expired User");
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var expiredSession = Session.Create("expired-token", DateTime.UtcNow.AddDays(-1), "127.0.0.1", "TestAgent");
        _dbContext.Sessions.Add(expiredSession);
        _dbContext.Entry(expiredSession).Property("UserId").CurrentValue = user.Id;
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        var command = new RefreshCommand("expired-token", "127.0.0.1", "TestAgent");

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidSession>().WithMessage("Session is no longer active");
    }

    [Fact]
    public async Task Handle_RevokedSession_ThrowsInvalidSession()
    {
        var user = User.Create("revoked@example.com", BCrypt.Net.BCrypt.HashPassword("pass"), "Revoked User");
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var session = Session.Create("revoked-token", DateTime.UtcNow.AddDays(7), "127.0.0.1", "TestAgent");
        session.Revoke();
        _dbContext.Sessions.Add(session);
        _dbContext.Entry(session).Property("UserId").CurrentValue = user.Id;
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        var command = new RefreshCommand("revoked-token", "127.0.0.1", "TestAgent");

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidSession>().WithMessage("Session is no longer active");
    }

    [Fact]
    public async Task Handle_ValidRefreshToken_CallsTokenService()
    {
        await SeedUserWithActiveSession();
        var command = new RefreshCommand("valid-token", "127.0.0.1", "TestAgent");

        await _handler.Handle(command, CancellationToken.None);

        _tokenService.Received(1).GenerateAccessToken(Arg.Any<User>(), Arg.Any<Guid?>());
        _tokenService.Received(1).GenerateRefreshToken();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}

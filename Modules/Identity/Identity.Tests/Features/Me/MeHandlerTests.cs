using FluentAssertions;
using Identity.Data;
using Identity.Identity.Exceptions;
using Identity.Identity.Features.Me;
using Identity.Identity.Models;
using Identity.Tests.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Stores.Contracts.Dtos;
using Stores.Contracts.Features;
using Xunit;

namespace Identity.Tests.Features.Me;

public class MeHandlerTests : IDisposable
{
    private readonly IdentityDbContext _dbContext;
    private readonly MeHandler _handler;
    private readonly ISender _sender;

    public MeHandlerTests()
    {
        _dbContext = DbContextFactory.Create();
        _sender = Substitute.For<ISender>();
        _sender.Send(Arg.Any<GetStoreByOwnerIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(new GetStoreByOwnerIdResult(null));
        _handler = new MeHandler(_dbContext, _sender);
    }

    private async Task<User> SeedUserWithSessions(int sessionCount = 0)
    {
        var user = User.Create("me@example.com", BCrypt.Net.BCrypt.HashPassword("pass"), "Me User");
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        for (var i = 0; i < sessionCount; i++)
        {
            var session = Session.Create($"token-{i}", DateTime.UtcNow.AddDays(7), "127.0.0.1", $"Agent-{i}");
            _dbContext.Sessions.Add(session);
            _dbContext.Entry(session).Property("UserId").CurrentValue = user.Id;
        }
        await _dbContext.SaveChangesAsync();

        return user;
    }

    [Fact]
    public async Task Handle_ValidUser_ReturnsUserDetails()
    {
        var user = await SeedUserWithSessions();
        var query = new MeQuery(user.Id);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.User.Id.Should().Be(user.Id);
        result.User.Email.Should().Be("me@example.com");
        result.User.DisplayName.Should().Be("Me User");
        result.User.IsActive.Should().BeTrue();
        result.User.EmailConfirmed.Should().BeFalse();
        result.User.Role.Should().Be("USER");
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsUserNotFound()
    {
        var query = new MeQuery(Guid.NewGuid());

        var act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<UserNotFound>().WithMessage("User not found");
    }

    [Fact]
    public async Task Handle_UserWithSessions_ReturnsSessions()
    {
        var user = await SeedUserWithSessions(sessionCount: 3);
        var query = new MeQuery(user.Id);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.User.Sessions.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_UserWithNoSessions_ReturnsEmptyList()
    {
        var user = await SeedUserWithSessions(sessionCount: 0);
        var query = new MeQuery(user.Id);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.User.Sessions.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_SessionDto_ContainsCorrectFields()
    {
        var user = await SeedUserWithSessions(sessionCount: 1);
        var query = new MeQuery(user.Id);

        var result = await _handler.Handle(query, CancellationToken.None);

        var session = result.User.Sessions.Single();
        session.Id.Should().NotBe(Guid.Empty);
        session.CreatedByIp.Should().Be("127.0.0.1");
        session.UserAgent.Should().Be("Agent-0");
        session.IsActive.Should().BeTrue();
        session.RevokedAt.Should().BeNull();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}

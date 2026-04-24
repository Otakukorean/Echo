using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Shared.Exceptions;

namespace Identity.Identity.Features.Me;

public class MeEndpoint : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/auth/me", async Task<Ok<MeResponse>> (
            ISender sender,
            HttpContext httpContext) =>
        {
            var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? httpContext.User.FindFirstValue("sub");

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                throw new UnauthorizedException("Invalid token");
            }

            var response = await sender.Send(new MeQuery(parsedUserId));
            return TypedResults.Ok(response);
        })
        .Produces<MeResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithTags("Auth")
        .WithName("Me")
        .RequireAuthorization();
    }
}

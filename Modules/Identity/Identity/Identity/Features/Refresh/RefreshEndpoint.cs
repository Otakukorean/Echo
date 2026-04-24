using Identity.Identity.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Identity.Identity.Features.Refresh;

public class RefreshEndpoint : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/refresh", async Task<Ok<RefreshResponse>> (
            ISender sender,
            HttpContext httpContext) =>
        {
            var refreshToken = httpContext.Request.Cookies["refresh_token"];
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new InvalidSession("Refresh token is missing");
            }

            var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var userAgent = httpContext.Request.Headers.UserAgent.ToString();

            var command = new RefreshCommand(refreshToken, ipAddress, userAgent);
            var result = await sender.Send(command);

            httpContext.Response.SetRefreshTokenCookie(result.RefreshToken, result.RefreshExpiresAt);

            return TypedResults.Ok(new RefreshResponse(result.AccessToken, result.ExpiresAt));
        })
        .Produces<RefreshResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithTags("Auth")
        .WithName("Refresh")
        .AllowAnonymous();
    }
}

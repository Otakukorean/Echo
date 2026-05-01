using Identity.Identity.Exceptions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Identity.Identity.Features.Logout;

public class LogoutEndpoint : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/logout", async Task<Ok<LogoutResponse>> (
            ISender sender,
            HttpContext httpContext) =>
        {
            var refreshToken = httpContext.Request.Cookies["refresh_token"];
            if (string.IsNullOrWhiteSpace(refreshToken))
                throw new InvalidSession("No active session");

            var result = await sender.Send(new LogoutCommand(refreshToken));

            // Clear the refresh token cookie
            httpContext.Response.Cookies.Delete("refresh_token", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/auth"
            });

            return TypedResults.Ok(result);
        })
        .Produces<LogoutResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithTags("Auth")
        .WithName("Logout")
        .RequireAuthorization();
    }
}

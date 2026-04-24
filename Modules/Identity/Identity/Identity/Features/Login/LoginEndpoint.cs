using Identity.Identity.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Identity.Identity.Features.Login;

public record LoginRequest(LoginDto LoginDto);

public class LoginEndpoint : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/login", async Task<Ok<LoginResponse>> (
            LoginRequest request,
            ISender sender,
            HttpContext httpContext) =>
        {
            var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var userAgent = httpContext.Request.Headers.UserAgent.ToString();

            var command = new LoginCommand(request.LoginDto, ipAddress, userAgent);
            var result = await sender.Send(command);

            httpContext.Response.SetRefreshTokenCookie(result.RefreshToken, result.RefreshExpiresAt);

            return TypedResults.Ok(new LoginResponse(result.AccessToken, result.ExpiresAt));
        })
        .Produces<LoginResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithTags("Auth")
        .WithName("Login")
        .AllowAnonymous();
    }
}

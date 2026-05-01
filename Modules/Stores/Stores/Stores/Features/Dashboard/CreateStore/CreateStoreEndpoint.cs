using System.Security.Claims;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Shared.Contracts.Identity;

namespace Stores.Stores.Features.Dashboard.CreateStore;

public class CreateStoreRequest
{
    [FromForm(Name = "name")]
    public string Name { get; set; } = default!;

    [FromForm(Name = "slug")]
    public string Slug { get; set; } = default!;

    [FromForm(Name = "description")]
    public string Description { get; set; } = default!;

    public IFormFile Logo { get; set; } = default!;

    public IFormFile? Cover { get; set; }
}

public record CreateStoreWithTokenResponse(StoreDto Store, string AccessToken, DateTime ExpiresAt);

public class CreateStoreEndpoint(ILogger<CreateStoreEndpoint> logger) : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/stores", async Task<Created<CreateStoreWithTokenResponse>> (
            [AsParameters] CreateStoreRequest request,
            ISender sender,
            IStoreContext storeContext,
            ITokenReissuer tokenReissuer,
            HttpContext httpContext) =>
        {
            logger.LogInformation("Creating store for user {UserId}", storeContext.UserId);

            var dto = new CreateStoreDto(
                request.Name,
                request.Slug,
                request.Description,
                request.Logo.OpenReadStream(),
                request.Logo.FileName,
                request.Logo.ContentType,
                request.Cover?.OpenReadStream(),
                request.Cover?.FileName,
                request.Cover?.ContentType);

            var response = await sender.Send(new CreateStoreCommand(dto, storeContext.UserId));

            var email = httpContext.User.FindFirstValue(ClaimTypes.Email)
                        ?? httpContext.User.FindFirstValue("email") ?? string.Empty;
            var role = httpContext.User.FindFirstValue("role") ?? "USER";

            var reissued = tokenReissuer.GenerateAccessToken(
                storeContext.UserId, email, role, response.Store.Id);

            return TypedResults.Created(
                $"/stores/{response.Store.Id}",
                new CreateStoreWithTokenResponse(response.Store, reissued.AccessToken, reissued.ExpiresAt));
        })
        .Produces<CreateStoreWithTokenResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithTags("Stores")
        .WithName("CreateStore")
        .DisableAntiforgery()
        .RequireAuthorization();
    }
}

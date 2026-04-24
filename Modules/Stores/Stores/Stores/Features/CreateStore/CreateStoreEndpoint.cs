using System.Security.Claims;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Shared.Exceptions;

namespace Stores.Stores.Features.CreateStore;

public record CreateStoreRequest(CreateStoreDto CreateStoreDto);

public class CreateStoreEndpoint : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/stores", async Task<Created<CreateStoreResponse>> (CreateStoreRequest request, ISender sender, HttpContext httpContext) =>
        {
            var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? httpContext.User.FindFirstValue("sub");

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                throw new UnauthorizedException("Invalid token");
            }

            var response = await sender.Send(new CreateStoreCommand(request.CreateStoreDto, parsedUserId));
            return TypedResults.Created($"/stores/{response.Store.Id}", response);
        })
        .Produces<CreateStoreResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithTags("Stores")
        .WithName("CreateStore")
        .RequireAuthorization();
    }
}

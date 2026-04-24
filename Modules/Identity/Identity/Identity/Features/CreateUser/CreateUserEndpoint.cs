using Microsoft.AspNetCore.Http.HttpResults;

namespace Identity.Identity.Features.CreateUser;

public record CreateUserRequest(RegisterDto RegisterDto);

public class CreateUserEndpoint : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/register", async Task<Created<CreateUserResponse>> (CreateUserRequest request, ISender sender) =>
        {
            var response = await sender.Send(new CreateUserCommand(request.RegisterDto));
            return TypedResults.Created("/auth/register", response);
        })
        .Produces<CreateUserResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithTags("Auth")
        .WithName("CreateUser")
        .AllowAnonymous();
    }
}
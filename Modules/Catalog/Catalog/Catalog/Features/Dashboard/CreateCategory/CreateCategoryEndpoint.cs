using Microsoft.AspNetCore.Http.HttpResults;

namespace Catalog.Catalog.Features.Dashboard.CreateCategory;

public record CreateCategoryRequest(CreateCategoryDto CreateCategoryDto);

public class CreateCategoryEndpoint : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/categories", async Task<Created<CreateCategoryResponse>> (
            CreateCategoryRequest request,
            ISender sender,
            IStoreContext storeContext) =>
        {
            var store = await storeContext.GetVerifiedStoreAsync();
            var response = await sender.Send(new CreateCategoryCommand(request.CreateCategoryDto, store.Id));
            return TypedResults.Created($"/categories/{response.Category.Id}", response);
        })
        .Produces<CreateCategoryResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithTags("Categories")
        .WithName("CreateCategory")
        .RequireAuthorization(PolicyNames.MustHaveStore);
    }
}

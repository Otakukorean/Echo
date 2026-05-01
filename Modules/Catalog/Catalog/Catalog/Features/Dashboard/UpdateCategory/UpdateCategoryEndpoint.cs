using Microsoft.AspNetCore.Mvc;

namespace Catalog.Catalog.Features.Dashboard.UpdateCategory;

public record UpdateCategoryRequestBody(UpdateCategoryDto UpdateCategoryDto);

public class UpdateCategoryEndpoint : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/categories/{categoryId:guid}", async (
            Guid categoryId,
            [FromBody] UpdateCategoryRequestBody request,
            ISender sender,
            IStoreContext storeContext) =>
        {
            var store = await storeContext.GetVerifiedStoreAsync();
            var response = await sender.Send(new UpdateCategoryCommand(request.UpdateCategoryDto, categoryId, store.Id));
            return Results.Ok(response);
        })
        .Produces<UpdateCategoryResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithTags("Categories")
        .WithName("UpdateCategory")
        .RequireAuthorization(PolicyNames.MustHaveStore);
    }
}

using Microsoft.AspNetCore.Http.HttpResults;

namespace Catalog.Catalog.Features.Dashboard.CreateProduct;

public record CreateProductRequest(CreateProductDto CreateProductDto);

public class CreateProductEndpoint : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/products", async Task<Created<CreateProductResponse>> (
            CreateProductRequest request,
            ISender sender,
            IStoreContext storeContext) =>
        {
            var store = await storeContext.GetVerifiedStoreAsync();
            var response = await sender.Send(new CreateProductCommand(request.CreateProductDto, store.Id));
            return TypedResults.Created($"/products/{response.Product.Id}", response);
        })
        .Produces<CreateProductResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithTags("Products")
        .WithName("CreateProduct")
        .RequireAuthorization(PolicyNames.MustHaveStore);
    }
}

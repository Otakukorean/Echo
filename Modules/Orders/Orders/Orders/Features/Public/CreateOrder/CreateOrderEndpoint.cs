using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Exceptions;
using Stores.Contracts.Features;

namespace Orders.Orders.Features.Public.CreateOrder;

public record CreateOrderRequest(CreateOrderDto CreateOrderDto);

public class CreateOrderEndpoint : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/storefront/{slug}/orders", async Task<Created<CreateOrderResponse>> (
            string slug,
            [FromBody] CreateOrderRequest request,
            ISender sender) =>
        {
            var storeResult = await sender.Send(new GetStoreBySlugQuery(slug));
            if (storeResult.Store is null)
                throw new NotFoundException("Store", slug);

            var response = await sender.Send(new CreateOrderCommand(request.CreateOrderDto, storeResult.Store.StoreId));

            return TypedResults.Created($"/orders/{response.Order.Id}", response);
        })
        .Produces<CreateOrderResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithTags("Storefront Orders")
        .WithName("CreateOrder")
        .AllowAnonymous();
    }
}

using Basket.Models.DomainModels;
using Basket.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Basket.Ednpoints;

public static class DecreaseQuantityEndpoint
{
    public static void MapDecreaseQuantityEndpoint(this IEndpointRouteBuilder endpoint)
    {
        endpoint.MapPatch("/primary/{item-id}/decrease",
             async ([FromRoute(Name = "item-id")] Guid ItemId,
             BasketDbContext dbContext) =>
             {

                 var primaryBasket = await dbContext.PrimaryUserBaskets
                                                    .Include(d => d.Items)
                                                    .FirstOrDefaultAsync(d => d.Items.Any(f => f.Id == ItemId));

                 if (primaryBasket is null)
                 {
                     return Results.BadRequest("invalid your data!");
                 }

                 primaryBasket.DecreaseQuantity(ItemId);
                 await dbContext.SaveChangesAsync();

                 return Results.Ok();
             });
    }
}

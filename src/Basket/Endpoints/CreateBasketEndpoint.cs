using Basket.Models.DomainModels;
using Basket.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Basket.Endpoints;

public static class CreateBasketEndpoint
{
    public static void MapCreateBasketEndpoint(this IEndpointRouteBuilder endpoint)
    {
        endpoint.MapPost("/primary/add-item",
             async (IUserPrincipal userPrincipal,
             IDistributedCache cache,
             HttpContent httpContext,
             CreateBasketItemRequest request,
             BasketDbContext dbContext) =>
             {

                 var primaryBasket = await dbContext.PrimaryUserBaskets
                                                    .FirstOrDefaultAsync(d => d.UserId == userPrincipal.IntUserId);

                 if (!userPrincipal.IsVerified)
                 {
                     var basketAsString = await cache.GetStringAsync(userPrincipal.UserId);
                     if (string.IsNullOrEmpty(basketAsString))
                     {
                         var tempBasket = new PrimaryUserBasket
                         {
                             UserId = userPrincipal.IntUserId,
                         };
                         await cache.SetStringAsync(userPrincipal.UserId, JsonSerializer.Serialize(tempBasket), new DistributedCacheEntryOptions
                         {
                             AbsoluteExpiration = DateTimeOffset.UtcNow.AddDays(1)
                         });
                     }
                     else
                     {
                         var basket = JsonSerializer.Deserialize<PrimaryUserBasket>(basketAsString);
                         basket.AddItem(request.Slug, request.Price, request.CatalogItemName);

                         await cache.SetStringAsync(userPrincipal.UserId, JsonSerializer.Serialize(basket), new DistributedCacheEntryOptions
                         {
                             AbsoluteExpiration = DateTimeOffset.Now.AddDays(1)
                         });
                     }
                     return Results.Ok();
                 }

                 if (primaryBasket is null)
                 {
                     primaryBasket = new PrimaryUserBasket
                     {
                         UserId = userPrincipal.IntUserId,
                     };
                     dbContext.UserBaskets.Add(primaryBasket);
                 }

                 primaryBasket.AddItem(request.Slug, request.Price, request.CatalogItemName);
                 await dbContext.SaveChangesAsync();

                 return Results.Ok();
             });
    }
}

public record CreateBasketItemRequest(string Slug, string CatalogItemName, decimal Price);

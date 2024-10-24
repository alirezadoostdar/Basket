namespace Basket.Subscription.PriceChanged;

public record PriceChangedEvent(string Slug, decimal Price);

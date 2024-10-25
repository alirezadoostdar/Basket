using Basket;
using Basket.Ednpoints;
using Basket.Endpoints;
using Basket.Persistence;
using Basket.Subscription.PriceChanged;
using Basket.Subscription.PrimaryBasketCleanup;
using MassTransit;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureDatabase();
builder.ConfigureBroker();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("redis");
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserPrincipal>(sp =>
{
    var httpContext = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;

    if(httpContext.Request.Headers.TryGetValue("user-id",out StringValues value))
    {
        return new UserPrincipal
        {
            UserId = $"vf{value}"
        };
    }

    var basketId = httpContext.Request.Cookies["basketId"];
    if(string.IsNullOrEmpty(basketId))
    {
        var userId = $"tb{Random.Shared.Next(1, int.MaxValue)}";
        var cookieOption = new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddHours(1),
            HttpOnly = false
        };
        httpContext.Response.Cookies.Append("basketId",userId, cookieOption);
        return new UserPrincipal
        {
            UserId = userId
        };
    }

    return new UserPrincipal
    {
        UserId = basketId
    };


});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapCreateBasketEndpoint();
app.MapMoveToNextEndpoint();
app.MapMoveToPrimaryEndpoint();
app.MapIncreaseQuantityEndpoint();
app.MapDecreaseQuantityEndpoint();
app.MapRemoveItemEndpoint();
app.MapBasketItemsEndpoint();


app.MapGet("/test/publish", (IPublishEndpoint publisher) =>
{
    publisher.Publish(new PriceChangedEvent("string-2", 10_000));
});


app.Run();


public static class ServiceActivationExtensions
{
    public static void ConfigureDatabase(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<BasketDbContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString(BasketDbContext.DefaultConnectionStringName));
        });
    }

    public static void ConfigureBroker(this WebApplicationBuilder builder)
    {
        builder.Services.AddMassTransit(configure =>
        {
            var brokerConfig = builder.Configuration.GetSection(BrokerOptions.SectionName)
                                                    .Get<BrokerOptions>();
            if (brokerConfig is null)
            {
                throw new ArgumentNullException(nameof(BrokerOptions));
            }

            configure.AddConsumers(Assembly.GetExecutingAssembly());
            configure.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(brokerConfig.Host, hostConfigure =>
                {
                    hostConfigure.Username(brokerConfig.Username);
                    hostConfigure.Password(brokerConfig.Password);
                });

                cfg.ConfigureEndpoints(context);
            });
        });
    }
}
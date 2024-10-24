using Basket.Models.DomainModels;
using Microsoft.EntityFrameworkCore;

namespace Basket.Persistence;

public class BasketDbContext(DbContextOptions dbContextOptions) :DbContext(dbContextOptions)
{
    public const string DefaultConnectionStringName = "SvcDbContext";

    public DbSet<UserBasket> UserBaskets { get; set; }
    public DbSet<PrimaryUserBasket> PrimaryUserBaskets { get; set; }
    public DbSet<SecondaryUserBasket> SecondaryUserBaskets { get; set; }

    public DbSet<BasketCatalogItem> BasketCatalogItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserBasket>().ToTable("UserBaskets");

        modelBuilder.Entity<UserBasket>().HasKey(d => d.Id);

        modelBuilder.Entity<UserBasket>().Property(x => x.UserId)
                                         .IsRequired();

        modelBuilder.Entity<UserBasket>().HasDiscriminator<string>("BasketType")
                                         .HasValue<PrimaryUserBasket>("Primary")
                                         .HasValue<SecondaryUserBasket>("Secondary");

        modelBuilder.Entity<UserBasket>()
                    .HasMany(d => d.Items)
                    .WithOne(f => f.UserBasket)
                    .HasForeignKey(f => f.UserBasketId)
                    .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BasketCatalogItem>().ToTable("BasketCatalogItems");

        modelBuilder.Entity<BasketCatalogItem>().HasKey(d => d.Id);

        modelBuilder.Entity<BasketCatalogItem>().Property(d => d.Quantity).IsRequired();
        modelBuilder.Entity<BasketCatalogItem>().Property(d => d.Slug)
                   .IsRequired()
                   .HasMaxLength(100)
                   .IsUnicode(false);

        modelBuilder.Entity<BasketCatalogItem>().Property(d => d.CatalogItemName)
                   .IsRequired()
                   .HasMaxLength(200)
                   .IsUnicode(true);

        modelBuilder.Entity<BasketCatalogItem>().Property(d => d.LatestPrice)
                   .IsRequired(false);

        modelBuilder.Entity<BasketCatalogItem>().Property(d => d.UserBasketId)
                   .IsRequired(true);

        modelBuilder.Entity<BasketCatalogItem>().Property(d => d.Price)
                   .IsRequired();

        modelBuilder.Entity<BasketCatalogItem>().Property(d => d.UserChangedSeen)
                   .IsRequired();

    }
}

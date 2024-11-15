using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WineShop.Models;

namespace WineShop.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<Manufacturer> Manufacturer { get; set; }
        public DbSet<Shipment> Shipment { get; set; }
        public DbSet<ProductType> ProductType { get; set; }
        public DbSet<PaymentMethod> PaymentMethod { get; set; }
        public DbSet<OrderStatus> OrderStatus { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<ApplicationUser> ApplicationUser { get; set; }
        public DbSet<Comment> Comment { get; set; }
        public DbSet<Rating> Rating { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = 1,
                    Name = "Cabernet Sauvignon",
                    Description = "A full-bodied red wine with rich notes of blackcurrant, subtle spices, and a smooth finish. Perfect for pairing with hearty meals.",
                    Price = 29.99M,
                    IdManufacturer = 1,
                    IdProductType = 1,
                    Image = "cabernet.png"
                },
                new Product
                {
                    Id = 2,
                    Name = "Chardonnay",
                    Description = "A crisp and refreshing white wine with delicate buttery undertones and a hint of citrus. Ideal for warm evenings or light dishes.",
                    Price = 19.99M,
                    IdManufacturer = 2,
                    IdProductType = 2,
                    Image = "chardonnay.png"
                },
                new Product
                {
                    Id = 3,
                    Name = "Prosecco",
                    Description = "A sparkling wine with vibrant bubbles and fresh flavors of green apple and pear. Celebrate life's moments with its joyful character.",
                    Price = 15.99M,
                    IdManufacturer = 2,
                    IdProductType = 3,
                    Image = "prosecco.png"
                }
            );

            modelBuilder.Entity<Manufacturer>().HasData(
                new Manufacturer
                {
                    Id = 1,
                    Name = "Red Vineyards",
                    Country = "France"
                },
                new Manufacturer
                {
                    Id = 2,
                    Name = "Golden Valley",
                    Country = "USA"
                }
            );

            modelBuilder.Entity<ProductType>().HasData(
                new ProductType
                {
                    Id = 1,
                    Name = "Red Wine"
                },
                new ProductType
                {
                    Id = 2,
                    Name = "White Wine"
                },
                new ProductType
                {
                    Id = 3,
                    Name = "Sparkling Wine"
                }
            );
        }

    }
}
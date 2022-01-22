using Microsoft.EntityFrameworkCore;
using WineShop.Models;

namespace WineShop.Data
{
    public class ApplicationDbContext : DbContext
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
    }
}

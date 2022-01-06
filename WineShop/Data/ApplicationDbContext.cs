using Microsoft.EntityFrameworkCore;
using WineShop.Models;

namespace WineShop.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<Category>? Categories { get; set; }
    }
}

using Microsoft.EntityFrameworkCore;
using WineShop.Data;
using WineShop.Models.ViewModels;
using WineShop.Services.Interfaces;

namespace WineShop.Services
{
    public class ProductDetailsService : IProductDetailsService
    {
        private readonly ApplicationDbContext _db;
        private readonly ICartService _cartService;
        private readonly IRatingService _ratingService;

        public ProductDetailsService(
            ApplicationDbContext db,
            ICartService cartService,
            IRatingService ratingService)
        {
            _db = db;
            _cartService = cartService;
            _ratingService = ratingService;
        }

        public async Task<DetailsVM?> GetAsync(int productId, string? userId)
        {
            var product = await _db.Product
                .AsNoTracking()
                .Include(x => x.ProductType)
                .Include(x => x.Manufacturer)
                .Include(x => x.Comment)
                .ThenInclude(x => x.ApplicationUser)
                .Include(x => x.Rating)
                .ThenInclude(x => x.ApplicationUser)
                .FirstOrDefaultAsync(x => x.Id == productId);

            if (product is null)
            {
                return null;
            }

            var vm = new DetailsVM
            {
                Product = product,
                ExistsInCart = _cartService.Contains(productId)
            };

            if (!string.IsNullOrEmpty(userId))
            {
                vm.UserRating = await _ratingService.GetUserRatingAsync(userId, productId);
            }

            return vm;
        }
    }
}
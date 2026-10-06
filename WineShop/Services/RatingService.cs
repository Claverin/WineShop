using Microsoft.EntityFrameworkCore;
using WineShop.Data;
using WineShop.Models;
using WineShop.Services.Interfaces;

namespace WineShop.Services
{
    public class RatingService : IRatingService
    {
        private readonly ApplicationDbContext _db;

        public RatingService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<int> GetUserRatingAsync(string userId, int productId)
        {
            var rating = await _db.Rating
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdCustomer == userId && x.IdProduct == productId);

            return rating?.RatingValue ?? 0;
        }

        public async Task SetRatingAsync(string userId, int productId, int rate)
        {
            var existing = await _db.Rating
                .FirstOrDefaultAsync(x => x.IdCustomer == userId && x.IdProduct == productId);

            if (rate == 0)
            {
                if (existing is not null)
                {
                    _db.Rating.Remove(existing);
                    await _db.SaveChangesAsync();
                }

                return;
            }

            if (existing is null)
            {
                _db.Rating.Add(new Rating
                {
                    IdCustomer = userId,
                    IdProduct = productId,
                    RatingValue = rate
                });
            }
            else
            {
                existing.RatingValue = rate;
                _db.Rating.Update(existing);
            }

            await _db.SaveChangesAsync();
        }
    }
}
namespace WineShop.Services.Interfaces
{
    public interface IRatingService
    {
        Task<int> GetUserRatingAsync(string userId, int productId);
        Task SetRatingAsync(string userId, int productId, int rate);
    }
}
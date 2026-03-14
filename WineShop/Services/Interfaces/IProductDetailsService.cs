using WineShop.Models.ViewModels;

namespace WineShop.Services.Interfaces
{
    public interface IProductDetailsService
    {
        Task<DetailsVM?> GetAsync(int productId, string? userId);
    }
}
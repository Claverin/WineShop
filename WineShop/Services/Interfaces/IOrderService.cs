using WineShop.Models.ViewModels;

namespace WineShop.Services.Interfaces
{
    public interface IOrderService
    {
        Task<CheckoutVM> BuildCheckoutAsync(string userId);
        Task<int?> PlaceOrderAsync(CheckoutVM model, string userId);
        Task<OrderConfirmationVM?> GetConfirmationAsync(int orderId, string userId, bool isAdmin);
    }
}
using WineShop.Models.ViewModels;

namespace WineShop.Services.Interfaces
{
    public interface IAdminOrderService
    {
        Task<List<AdminOrderListItemVM>> GetOrdersAsync();
        Task<AdminOrderDetailsVM?> GetOrderAsync(int orderId);
        Task<bool> UpdateOrderAsync(AdminOrderDetailsVM model);
    }
}
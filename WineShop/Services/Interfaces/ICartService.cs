using WineShop.Models;

namespace WineShop.Services.Interfaces
{
    public interface ICartService
    {
        bool Contains(int productId);
        int GetQuantity(int productId);
        void Add(int productId, int quantity = 1);
        void Increase(int productId);
        void Decrease(int productId);
        void Remove(int productId);
        List<ShoppingCart> GetAll();
        List<int> GetProductIds();
        void Clear();
    }
}

using WineShop.Models;

namespace WineShop.Services.Interfaces
{
    public interface ICartService
    {
        bool Contains(int productId);
        void Add(int productId);
        void Remove(int productId);
        List<ShoppingCart> GetAll();
        List<int> GetProductIds();
        void Clear();
    }
}
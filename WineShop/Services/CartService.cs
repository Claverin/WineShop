using Microsoft.AspNetCore.Http;
using WineShop.Models;
using WineShop.Services.Interfaces;
using WineShop.Utility;

namespace WineShop.Services
{
    public class CartService : ICartService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ISession Session => _httpContextAccessor.HttpContext!.Session;

        public bool Contains(int productId)
        {
            return CartSession.Contains(Session, productId);
        }

        public int GetQuantity(int productId)
        {
            return CartSession.GetQuantity(Session, productId);
        }

        public void Add(int productId, int quantity = 1)
        {
            CartSession.Add(Session, productId, quantity);
        }

        public void Increase(int productId)
        {
            CartSession.Increase(Session, productId);
        }

        public void Decrease(int productId)
        {
            CartSession.Decrease(Session, productId);
        }

        public void Remove(int productId)
        {
            CartSession.Remove(Session, productId);
        }

        public List<ShoppingCart> GetAll()
        {
            return CartSession.GetCart(Session);
        }

        public List<int> GetProductIds()
        {
            return CartSession.GetProductIds(Session);
        }

        public void Clear()
        {
            Session.Remove(WC.SessionCart);
        }
    }
}

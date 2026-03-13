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

        public void Add(int productId)
        {
            CartSession.Add(Session, productId);
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
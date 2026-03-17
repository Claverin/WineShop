using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using WineShop.Models;

namespace WineShop.Utility
{
    public static class CartSession
    {
        public static List<ShoppingCart> GetCart(ISession session)
        {
            var cartJson = session.GetString(WC.SessionCart);

            if (string.IsNullOrEmpty(cartJson))
            {
                return new List<ShoppingCart>();
            }

            return JsonConvert.DeserializeObject<List<ShoppingCart>>(cartJson) ?? new List<ShoppingCart>();
        }

        public static void SetCart(ISession session, List<ShoppingCart> cart)
        {
            session.SetString(WC.SessionCart, JsonConvert.SerializeObject(cart));
        }

        public static List<int> GetProductIds(ISession session)
        {
            return GetCart(session)
                .Select(x => x.ProductId)
                .ToList();
        }

        public static bool Contains(ISession session, int productId)
        {
            return GetCart(session).Any(x => x.ProductId == productId);
        }

        public static int GetQuantity(ISession session, int productId)
        {
            return GetCart(session)
                .FirstOrDefault(x => x.ProductId == productId)?.Quantity ?? 0;
        }

        public static void Add(ISession session, int productId, int quantity = 1)
        {
            var cart = GetCart(session);
            var item = cart.FirstOrDefault(x => x.ProductId == productId);

            if (item is null)
            {
                cart.Add(new ShoppingCart
                {
                    ProductId = productId,
                    Quantity = Math.Max(1, quantity)
                });
            }
            else
            {
                item.Quantity += Math.Max(1, quantity);
            }

            SetCart(session, cart);
        }

        public static void Increase(ISession session, int productId)
        {
            var cart = GetCart(session);
            var item = cart.FirstOrDefault(x => x.ProductId == productId);

            if (item is null)
            {
                cart.Add(new ShoppingCart
                {
                    ProductId = productId,
                    Quantity = 1
                });
            }
            else
            {
                item.Quantity++;
            }

            SetCart(session, cart);
        }

        public static void Decrease(ISession session, int productId)
        {
            var cart = GetCart(session);
            var item = cart.FirstOrDefault(x => x.ProductId == productId);

            if (item is null)
            {
                return;
            }

            item.Quantity--;

            if (item.Quantity <= 0)
            {
                cart.Remove(item);
            }

            SetCart(session, cart);
        }

        public static void Remove(ISession session, int productId)
        {
            var cart = GetCart(session);
            var item = cart.FirstOrDefault(x => x.ProductId == productId);

            if (item is null)
            {
                return;
            }

            cart.Remove(item);
            SetCart(session, cart);
        }
    }
}

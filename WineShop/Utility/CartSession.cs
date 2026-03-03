using Microsoft.AspNetCore.Http;
using WineShop.Models;

namespace WineShop.Utility;

public static class CartSession
{
    public static List<ShoppingCart> GetCart(ISession session)
        => session.Get<List<ShoppingCart>>(WC.SessionCart) ?? new List<ShoppingCart>();

    public static void SaveCart(ISession session, List<ShoppingCart> cart)
        => session.Set(WC.SessionCart, cart);

    public static bool Contains(ISession session, int productId)
        => GetCart(session).Any(x => x.ProductId == productId);

    public static void Add(ISession session, int productId)
    {
        var cart = GetCart(session);
        if (cart.Any(x => x.ProductId == productId))
            return;

        cart.Add(new ShoppingCart { ProductId = productId });
        SaveCart(session, cart);
    }

    public static void Remove(ISession session, int productId)
    {
        var cart = GetCart(session);
        var item = cart.FirstOrDefault(x => x.ProductId == productId);
        if (item is null) return;

        cart.Remove(item);
        SaveCart(session, cart);
    }

    public static List<int> GetProductIds(ISession session)
        => GetCart(session).Select(x => x.ProductId).Distinct().ToList();
}
using Microsoft.AspNetCore.Http;
using Xunit;
using WineShop.Models;
using WineShop.Utility;
using System.Collections.Generic;

namespace WineShop.Tests.Utility
{
    public class CartSessionTests
    {
        [Fact]
        public void GetCart_ShouldReturnEmptyList_WhenSessionIsEmpty()
        {
            // Arrange
            var session = new FakeSession();

            // Act
            var cart = CartSession.GetCart(session);

            // Assert
            Assert.NotNull(cart);
            Assert.Empty(cart);
        }

        [Fact]
        public void GetCart_ShouldReturnEmptyList_WhenDeserializationReturnsNull()
        {
            // Arrange
            var session = new FakeSession();
            session.SetString(WC.SessionCart, "null");

            // Act
            var cart = CartSession.GetCart(session);

            // Assert
            Assert.NotNull(cart);
            Assert.Empty(cart);
        }

        [Fact]
        public void SetCartAndGetCart_ShouldStoreAndRetrieveCartSuccessfully()
        {
            // Arrange
            var session = new FakeSession();
            var cart = new List<ShoppingCart> { new ShoppingCart { ProductId = 1, Quantity = 2 } };

            // Act
            CartSession.SetCart(session, cart);
            var retrievedCart = CartSession.GetCart(session);

            // Assert
            Assert.Single(retrievedCart);
            Assert.Equal(1, retrievedCart[0].ProductId);
            Assert.Equal(2, retrievedCart[0].Quantity);
        }

        [Fact]
        public void GetProductIds_ShouldReturnListOfIds()
        {
            // Arrange
            var session = new FakeSession();
            var cart = new List<ShoppingCart>
            {
                new ShoppingCart { ProductId = 1 },
                new ShoppingCart { ProductId = 5 }
            };
            CartSession.SetCart(session, cart);

            // Act
            var ids = CartSession.GetProductIds(session);

            // Assert
            Assert.Equal(2, ids.Count);
            Assert.Contains(1, ids);
            Assert.Contains(5, ids);
        }

        [Fact]
        public void Contains_ShouldReturnTrue_WhenProductExists()
        {
            // Arrange
            var session = new FakeSession();
            CartSession.SetCart(session, new List<ShoppingCart> { new ShoppingCart { ProductId = 3 } });

            // Act
            var result = CartSession.Contains(session, 3);
            var resultFalse = CartSession.Contains(session, 99);

            // Assert
            Assert.True(result);
            Assert.False(resultFalse);
        }

        [Fact]
        public void GetQuantity_ShouldReturnCorrectQuantityOrZero()
        {
            // Arrange
            var session = new FakeSession();
            CartSession.SetCart(session, new List<ShoppingCart> { new ShoppingCart { ProductId = 4, Quantity = 5 } });

            // Act
            var qtyExisting = CartSession.GetQuantity(session, 4);
            var qtyMissing = CartSession.GetQuantity(session, 99);

            // Assert
            Assert.Equal(5, qtyExisting);
            Assert.Equal(0, qtyMissing);
        }

        [Fact]
        public void Add_ShouldAddNewItem_WhenProductNotInCart()
        {
            // Arrange
            var session = new FakeSession();

            // Act
            CartSession.Add(session, 1, 2);

            // Assert
            var cart = CartSession.GetCart(session);
            Assert.Single(cart);
            Assert.Equal(2, cart[0].Quantity);
        }

        [Fact]
        public void Add_ShouldIncreaseQuantity_WhenProductAlreadyInCart()
        {
            // Arrange
            var session = new FakeSession();
            CartSession.SetCart(session, new List<ShoppingCart> { new ShoppingCart { ProductId = 1, Quantity = 2 } });

            // Act
            CartSession.Add(session, 1, 3);

            // Assert
            var cart = CartSession.GetCart(session);
            Assert.Equal(5, cart[0].Quantity);
        }

        [Fact]
        public void Add_ShouldEnforceMinimumQuantityOfOne()
        {
            // Arrange
            var session = new FakeSession();

            // Act
            CartSession.Add(session, 1, -5);

            // Assert
            var cart = CartSession.GetCart(session);
            Assert.Equal(1, cart[0].Quantity);
        }

        [Fact]
        public void Increase_ShouldAddNewItem_WhenProductNotInCart()
        {
            // Arrange
            var session = new FakeSession();

            // Act
            CartSession.Increase(session, 2);

            // Assert
            var cart = CartSession.GetCart(session);
            Assert.Single(cart);
            Assert.Equal(1, cart[0].Quantity);
        }

        [Fact]
        public void Increase_ShouldIncrementQuantity_WhenProductAlreadyInCart()
        {
            // Arrange
            var session = new FakeSession();
            CartSession.SetCart(session, new List<ShoppingCart> { new ShoppingCart { ProductId = 2, Quantity = 2 } });

            // Act
            CartSession.Increase(session, 2);

            // Assert
            var cart = CartSession.GetCart(session);
            Assert.Equal(3, cart[0].Quantity);
        }

        [Fact]
        public void Decrease_ShouldDoNothing_WhenProductNotInCart()
        {
            // Arrange
            var session = new FakeSession();

            // Act
            CartSession.Decrease(session, 1);

            // Assert
            var cart = CartSession.GetCart(session);
            Assert.Empty(cart);
        }

        [Fact]
        public void Decrease_ShouldDecrementQuantity_WhenQuantityIsGreaterThanOne()
        {
            // Arrange
            var session = new FakeSession();
            CartSession.SetCart(session, new List<ShoppingCart> { new ShoppingCart { ProductId = 1, Quantity = 3 } });

            // Act
            CartSession.Decrease(session, 1);

            // Assert
            var cart = CartSession.GetCart(session);
            Assert.Equal(2, cart[0].Quantity);
        }

        [Fact]
        public void Decrease_ShouldRemoveItem_WhenQuantityReachesZero()
        {
            // Arrange
            var session = new FakeSession();
            CartSession.SetCart(session, new List<ShoppingCart> { new ShoppingCart { ProductId = 1, Quantity = 1 } });

            // Act
            CartSession.Decrease(session, 1);

            // Assert
            var cart = CartSession.GetCart(session);
            Assert.Empty(cart);
        }

        [Fact]
        public void Remove_ShouldDoNothing_WhenProductNotInCart()
        {
            // Arrange
            var session = new FakeSession();

            // Act
            CartSession.Remove(session, 1);

            // Assert
            var cart = CartSession.GetCart(session);
            Assert.Empty(cart);
        }

        [Fact]
        public void Remove_ShouldRemoveItem_WhenProductInCart()
        {
            // Arrange
            var session = new FakeSession();
            CartSession.SetCart(session, new List<ShoppingCart> { new ShoppingCart { ProductId = 1, Quantity = 5 } });

            // Act
            CartSession.Remove(session, 1);

            // Assert
            var cart = CartSession.GetCart(session);
            Assert.Empty(cart);
        }
    }
}
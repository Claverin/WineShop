using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using WineShop.Services;
using WineShop.Models;
using WineShop.Utility;
using System.Collections.Generic;

namespace WineShop.Tests.Services
{
    public class CartServiceTests
    {
        private (CartService service, Mock<ISession> sessionMock) GetServiceWithMockSession()
        {
            var sessionMock = new Mock<ISession>();
            var storage = new Dictionary<string, byte[]>();

            sessionMock.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
                .Callback<string, byte[]>((key, val) => storage[key] = val);

            sessionMock.Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) => {
                    return storage.TryGetValue(key, out value);
                });

            sessionMock.Setup(s => s.Remove(It.IsAny<string>()))
                .Callback<string>(key => storage.Remove(key));

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(h => h.Session).Returns(sessionMock.Object);

            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContextMock.Object);

            var cartService = new CartService(httpContextAccessorMock.Object);
            return (cartService, sessionMock);
        }

        [Fact]
        public void Add_ShouldAddProductToCart()
        {
            // Arrange
            var (service, _) = GetServiceWithMockSession();

            // Act
            service.Add(1, 2);

            // Assert
            var items = service.GetAll();
            Assert.Single(items);
            Assert.Equal(1, items[0].ProductId);
            Assert.Equal(2, items[0].Quantity);
        }

        [Fact]
        public void Contains_ShouldReturnTrue_WhenProductExistsInCart()
        {
            // Arrange
            var (service, _) = GetServiceWithMockSession();
            service.Add(5, 1);

            // Act
            var result = service.Contains(5);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void GetQuantity_ShouldReturnCorrectQuantity()
        {
            // Arrange
            var (service, _) = GetServiceWithMockSession();
            service.Add(3, 4);

            // Act
            var quantity = service.GetQuantity(3);

            // Assert
            Assert.Equal(4, quantity);
        }

        [Fact]
        public void Increase_ShouldIncrementProductQuantity()
        {
            // Arrange
            var (service, _) = GetServiceWithMockSession();
            service.Add(2, 1);

            // Act
            service.Increase(2);

            // Assert
            Assert.Equal(2, service.GetQuantity(2));
        }

        [Fact]
        public void Decrease_ShouldDecrementProductQuantity()
        {
            // Arrange
            var (service, _) = GetServiceWithMockSession();
            service.Add(2, 2);

            // Act
            service.Decrease(2);

            // Assert
            Assert.Equal(1, service.GetQuantity(2));
        }

        [Fact]
        public void Remove_ShouldDeleteProductFromCart()
        {
            // Arrange
            var (service, _) = GetServiceWithMockSession();
            service.Add(1, 1);
            service.Add(2, 1);

            // Act
            service.Remove(1);

            // Assert
            var products = service.GetProductIds();
            Assert.Single(products);
            Assert.DoesNotContain(1, products);
            Assert.Contains(2, products);
        }

        [Fact]
        public void Clear_ShouldRemoveCartSession()
        {
            // Arrange
            var (service, sessionMock) = GetServiceWithMockSession();
            service.Add(1, 1);

            // Act
            service.Clear();

            // Assert
            sessionMock.Verify(s => s.Remove(WC.SessionCart), Times.Once);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using WineShop.Data;
using WineShop.Models;
using WineShop.Models.ViewModels;
using WineShop.Services;
using WineShop.Services.Interfaces;
using Xunit;

namespace WineShop.Tests.Services
{
    public class OrderServiceTests
    {
        private ApplicationDbContext GetInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task BuildCheckoutAsync_ShouldReturnCorrectViewModel_WhenFullDataExists()
        {
            // Arrange
            var dbName = nameof(BuildCheckoutAsync_ShouldReturnCorrectViewModel_WhenFullDataExists);
            using var db = GetInMemoryDbContext(dbName);

            db.ApplicationUsers.Add(new ApplicationUser
            {
                Id = "user-1",
                UserName = "john.doe@example.com",
                NormalizedUserName = "JOHN.DOE@EXAMPLE.COM",
                Email = "john.doe@example.com",
                NormalizedEmail = "JOHN.DOE@EXAMPLE.COM",
                Name = "John",
                Surname = "Doe",
                PhoneNumber = "123456789"
            });

            db.PaymentMethod.Add(new PaymentMethod { Id = 1, Name = "Credit Card" });
            db.Product.Add(new Product { Id = 10, Name = "Red Wine", Price = 50.0m, Image = "wine.jpg" });
            await db.SaveChangesAsync();

            var cartServiceMock = new Mock<ICartService>();
            cartServiceMock.Setup(c => c.GetAll()).Returns(new List<ShoppingCart>
            {
                new ShoppingCart { ProductId = 10, Quantity = 2 }
            });

            var service = new OrderService(db, cartServiceMock.Object);

            // Act
            var result = await service.BuildCheckoutAsync("user-1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("John Doe", result.Name);
            Assert.Equal("john.doe@example.com", result.Email);
            Assert.Equal("123456789", result.PhoneNumber);
            Assert.Equal(1, result.PaymentMethodId);
            Assert.Single(result.PaymentMethods);
            Assert.Single(result.Items);

            var item = result.Items.First();
            Assert.Equal(10, item.ProductId);
            Assert.Equal("Red Wine", item.Name);
            Assert.Equal(50.0m, item.Price);
            Assert.Equal(2, item.Quantity);
        }

        [Fact]
        public async Task BuildCheckoutAsync_ShouldFallbackToUserName_WhenUserIsNullOrFailNameIsEmpty()
        {
            // Arrange
            var dbName = nameof(BuildCheckoutAsync_ShouldFallbackToUserName_WhenUserIsNullOrFailNameIsEmpty);
            using var db = GetInMemoryDbContext(dbName);

            db.ApplicationUsers.Add(new ApplicationUser
            {
                Id = "user-fallback",
                UserName = "fallback_user",
                NormalizedUserName = "FALLBACK_USER",
                Email = "fallback@example.com",
                NormalizedEmail = "FALLBACK@EXAMPLE.COM",
                Name = string.Empty,
                Surname = string.Empty
            });

            await db.SaveChangesAsync();

            var cartServiceMock = new Mock<ICartService>();
            cartServiceMock.Setup(c => c.GetAll()).Returns(new List<ShoppingCart>());

            var service = new OrderService(db, cartServiceMock.Object);

            // Act
            var result = await service.BuildCheckoutAsync("user-fallback");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("fallback_user", result.Name);
        }

        [Fact]
        public async Task BuildCheckoutAsync_ShouldHandleMissingUserAndEmptyPaymentMethods()
        {
            // Arrange
            var dbName = nameof(BuildCheckoutAsync_ShouldHandleMissingUserAndEmptyPaymentMethods);
            using var db = GetInMemoryDbContext(dbName);

            var cartServiceMock = new Mock<ICartService>();
            cartServiceMock.Setup(c => c.GetAll()).Returns(new List<ShoppingCart>
            {
                new ShoppingCart { ProductId = 999, Quantity = 1 }
            });

            var service = new OrderService(db, cartServiceMock.Object);

            // Act
            var result = await service.BuildCheckoutAsync("non-existent-user");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(string.Empty, result.Name);
            Assert.Equal(string.Empty, result.Email);
            Assert.Equal(string.Empty, result.PhoneNumber);
            Assert.Equal(0, result.PaymentMethodId);
            Assert.Empty(result.PaymentMethods);
            Assert.Empty(result.Items);
        }

        [Fact]
        public async Task PlaceOrderAsync_ShouldReturnNull_WhenCartIsEmpty()
        {
            // Arrange
            var dbName = nameof(PlaceOrderAsync_ShouldReturnNull_WhenCartIsEmpty);
            using var db = GetInMemoryDbContext(dbName);

            var cartServiceMock = new Mock<ICartService>();
            cartServiceMock.Setup(c => c.GetAll()).Returns(new List<ShoppingCart>());

            var service = new OrderService(db, cartServiceMock.Object);
            var model = new CheckoutVM();

            // Act
            var result = await service.PlaceOrderAsync(model, "user-1");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task PlaceOrderAsync_ShouldReturnNull_WhenPaymentMethodDoesNotExist()
        {
            // Arrange
            var dbName = nameof(PlaceOrderAsync_ShouldReturnNull_WhenPaymentMethodDoesNotExist);
            using var db = GetInMemoryDbContext(dbName);

            var cartServiceMock = new Mock<ICartService>();
            cartServiceMock.Setup(c => c.GetAll()).Returns(new List<ShoppingCart>
            {
                new ShoppingCart { ProductId = 1, Quantity = 1 }
            });

            var service = new OrderService(db, cartServiceMock.Object);
            var model = new CheckoutVM { PaymentMethodId = 99 };

            // Act
            var result = await service.PlaceOrderAsync(model, "user-1");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task PlaceOrderAsync_ShouldReturnNull_WhenPendingStatusDoesNotExist()
        {
            // Arrange
            var dbName = nameof(PlaceOrderAsync_ShouldReturnNull_WhenPendingStatusDoesNotExist);
            using var db = GetInMemoryDbContext(dbName);

            db.PaymentMethod.Add(new PaymentMethod { Id = 1, Name = "PayPal" });
            await db.SaveChangesAsync();

            var cartServiceMock = new Mock<ICartService>();
            cartServiceMock.Setup(c => c.GetAll()).Returns(new List<ShoppingCart>
            {
                new ShoppingCart { ProductId = 1, Quantity = 1 }
            });

            var service = new OrderService(db, cartServiceMock.Object);
            var model = new CheckoutVM { PaymentMethodId = 1 };

            // Act
            var result = await service.PlaceOrderAsync(model, "user-1");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task PlaceOrderAsync_ShouldReturnNull_WhenProductsDoNotExistInDatabase()
        {
            // Arrange
            var dbName = nameof(PlaceOrderAsync_ShouldReturnNull_WhenProductsDoNotExistInDatabase);
            using var db = GetInMemoryDbContext(dbName);

            db.PaymentMethod.Add(new PaymentMethod { Id = 1, Name = "PayPal" });
            db.OrderStatus.Add(new OrderStatus { Id = 1, Name = "Pending" });
            await db.SaveChangesAsync();

            var cartServiceMock = new Mock<ICartService>();
            cartServiceMock.Setup(c => c.GetAll()).Returns(new List<ShoppingCart>
            {
                new ShoppingCart { ProductId = 999, Quantity = 1 }
            });

            var service = new OrderService(db, cartServiceMock.Object);
            var model = new CheckoutVM { PaymentMethodId = 1 };

            // Act
            var result = await service.PlaceOrderAsync(model, "user-1");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task PlaceOrderAsync_ShouldCreateOrderAndClearCart_OnSuccess()
        {
            // Arrange
            var dbName = nameof(PlaceOrderAsync_ShouldCreateOrderAndClearCart_OnSuccess);
            using var db = GetInMemoryDbContext(dbName);

            db.PaymentMethod.Add(new PaymentMethod { Id = 1, Name = "Cash on Delivery" });
            db.OrderStatus.Add(new OrderStatus { Id = 1, Name = "Pending" });
            db.Product.Add(new Product { Id = 5, Name = "White Wine", Price = 40.0m, Image = "white.jpg" });
            await db.SaveChangesAsync();

            var cartServiceMock = new Mock<ICartService>();
            cartServiceMock.Setup(c => c.GetAll()).Returns(new List<ShoppingCart>
            {
                new ShoppingCart { ProductId = 5, Quantity = 3 }
            });

            var service = new OrderService(db, cartServiceMock.Object);
            var model = new CheckoutVM
            {
                Name = "Jane Smith",
                Email = "jane.smith@example.com",
                PhoneNumber = "987654321",
                Street = "Main Street 1",
                PostalCode = "00-001",
                City = "New York",
                PaymentMethodId = 1
            };

            // Act
            var orderId = await service.PlaceOrderAsync(model, "user-2");

            // Assert
            Assert.NotNull(orderId);
            Assert.True(orderId > 0);

            var savedOrder = await db.Order.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId);
            Assert.NotNull(savedOrder);
            Assert.Equal("user-2", savedOrder.CustomerId);
            Assert.Equal(120.0m, savedOrder.TotalAmount);
            Assert.Single(savedOrder.Items);
            Assert.Equal("White Wine", savedOrder.Items.First().ProductName);

            cartServiceMock.Verify(c => c.Clear(), Times.Once);
        }

        [Fact]
        public async Task GetConfirmationAsync_ShouldReturnNull_WhenOrderDoesNotExist()
        {
            // Arrange
            var dbName = nameof(GetConfirmationAsync_ShouldReturnNull_WhenOrderDoesNotExist);
            using var db = GetInMemoryDbContext(dbName);

            var cartServiceMock = new Mock<ICartService>();
            var service = new OrderService(db, cartServiceMock.Object);

            // Act
            var result = await service.GetConfirmationAsync(999, "user-1", false);

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData("user-1", "user-1", false, true)]
        [InlineData("user-1", "user-2", false, false)]
        [InlineData("user-1", "user-2", true, true)]
        public async Task GetConfirmationAsync_ShouldRespectPermissions(string orderUserId, string requestUserId, bool isAdmin, bool shouldExist)
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var db = GetInMemoryDbContext(dbName);

            db.OrderStatus.Add(new OrderStatus { Id = 1, Name = "Completed" });
            db.Order.Add(new Order
            {
                Id = 100,
                CustomerId = orderUserId,
                CustomerName = "Test User",
                CustomerEmail = "test@example.com",
                CustomerPhoneNumber = "123456789",
                Street = "Test St",
                PostalCode = "00-000",
                City = "Test City",
                PaymentMethodId = 1,
                OrderStatusId = 1,
                TotalAmount = 50.0m,
                CreatedAtUtc = DateTime.UtcNow
            });
            await db.SaveChangesAsync();

            var cartServiceMock = new Mock<ICartService>();
            var service = new OrderService(db, cartServiceMock.Object);

            // Act
            var result = await service.GetConfirmationAsync(100, requestUserId, isAdmin);

            // Assert
            if (shouldExist)
            {
                Assert.NotNull(result);
                Assert.Equal(100, result.OrderId);
                Assert.Equal("Completed", result.StatusName);
            }
            else
            {
                Assert.Null(result);
            }
        }
    }
}
using Microsoft.EntityFrameworkCore;
using Xunit;
using WineShop.Data;
using WineShop.Models;
using WineShop.Models.ViewModels;
using WineShop.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WineShop.Tests.Services
{
    public class AdminOrderServiceTests
    {
        private ApplicationDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetOrdersAsync_ShouldReturnMappedOrders_OrderedByDateDescending()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using (var dbContextSetup = GetDbContext(dbName))
            {
                dbContextSetup.OrderStatus.Add(new OrderStatus { Id = 1, Name = "New" });
                dbContextSetup.PaymentMethod.Add(new PaymentMethod { Id = 1, Name = "Card" });

                dbContextSetup.Order.Add(new Order
                {
                    Id = 1,
                    CreatedAtUtc = new DateTime(2023, 1, 1),
                    CustomerId = "user-1",
                    CustomerName = "John Doe",
                    CustomerEmail = "john@test.com",
                    CustomerPhoneNumber = "123",
                    Street = "A",
                    PostalCode = "B",
                    City = "C",
                    PaymentMethodId = 1,
                    OrderStatusId = 1,
                    TotalAmount = 100
                });

                dbContextSetup.Order.Add(new Order
                {
                    Id = 2,
                    CreatedAtUtc = new DateTime(2023, 1, 2),
                    CustomerId = "user-2",
                    CustomerName = "Jane Doe",
                    CustomerEmail = "jane@test.com",
                    CustomerPhoneNumber = "321",
                    Street = "X",
                    PostalCode = "Y",
                    City = "Z",
                    PaymentMethodId = 1,
                    OrderStatusId = 1,
                    TotalAmount = 200,
                    Carrier = "DHL"
                });

                dbContextSetup.SaveChanges();
            }

            using var dbContext = GetDbContext(dbName);
            var service = new AdminOrderService(dbContext);

            // Act
            var result = await service.GetOrdersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(2, result[0].OrderId);
            Assert.Equal(1, result[1].OrderId);
            Assert.True(result[0].HasShippingInfo);
            Assert.False(result[1].HasShippingInfo);
            Assert.Equal("Card", result[0].PaymentMethodName);
            Assert.Equal("New", result[0].StatusName);
        }

        [Fact]
        public async Task GetOrderAsync_ShouldReturnNull_WhenOrderDoesNotExist()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var dbContext = GetDbContext(dbName);
            var service = new AdminOrderService(dbContext);

            // Act
            var result = await service.GetOrderAsync(99);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetOrderAsync_ShouldReturnOrderDetails_WhenOrderExists()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using (var dbContextSetup = GetDbContext(dbName))
            {
                dbContextSetup.OrderStatus.Add(new OrderStatus { Id = 1, Name = "Status A" });
                dbContextSetup.OrderStatus.Add(new OrderStatus { Id = 2, Name = "Status B" });
                dbContextSetup.PaymentMethod.Add(new PaymentMethod { Id = 1, Name = "Card" });

                dbContextSetup.Manufacturer.Add(new Manufacturer { Id = 1, Name = "M1", Country = "US" });
                dbContextSetup.ProductType.Add(new ProductType { Id = 1, Name = "T1" });
                dbContextSetup.Product.Add(new Product { Id = 1, Name = "Wine", Price = 50, Image = "w.png", IdManufacturer = 1, IdProductType = 1 });

                var order = new Order
                {
                    Id = 1,
                    CreatedAtUtc = DateTime.UtcNow,
                    CustomerId = "user-1",
                    CustomerName = "John Doe",
                    CustomerEmail = "john@test.com",
                    CustomerPhoneNumber = "123",
                    Street = "Test",
                    PostalCode = "00-000",
                    City = "City",
                    PaymentMethodId = 1,
                    OrderStatusId = 1,
                    TotalAmount = 100,
                    Items = new List<OrderItem>
                    {
                        new OrderItem { Id = 1, ProductId = 1, ProductName = "Wine", UnitPrice = 50, Quantity = 2 }
                    }
                };

                dbContextSetup.Order.Add(order);
                dbContextSetup.SaveChanges();
            }

            using var dbContext = GetDbContext(dbName);
            var service = new AdminOrderService(dbContext);

            // Act
            var result = await service.GetOrderAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.OrderId);
            Assert.Equal("John Doe", result.CustomerName);
            Assert.Equal("Status A", result.CurrentStatusName);
            Assert.Equal("Card", result.PaymentMethodName);
            Assert.Single(result.Items);
            Assert.Equal("Wine", result.Items.First().ProductName);
            Assert.Equal(2, result.Statuses.Count());
        }

        [Fact]
        public async Task UpdateOrderAsync_ShouldReturnFalse_WhenOrderDoesNotExist()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var dbContext = GetDbContext(dbName);
            var service = new AdminOrderService(dbContext);
            var model = new AdminOrderDetailsVM { OrderId = 99 };

            // Act
            var result = await service.UpdateOrderAsync(model);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateOrderAsync_ShouldReturnFalse_WhenStatusDoesNotExist()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using (var dbContextSetup = GetDbContext(dbName))
            {
                dbContextSetup.Order.Add(new Order
                {
                    Id = 1,
                    CustomerId = "u",
                    CustomerName = "n",
                    CustomerEmail = "e",
                    CustomerPhoneNumber = "1",
                    Street = "s",
                    PostalCode = "p",
                    City = "c",
                    PaymentMethodId = 1,
                    OrderStatusId = 1,
                    TotalAmount = 10
                });
                dbContextSetup.SaveChanges();
            }

            using var dbContext = GetDbContext(dbName);
            var service = new AdminOrderService(dbContext);
            var model = new AdminOrderDetailsVM { OrderId = 1, OrderStatusId = 99 };

            // Act
            var result = await service.UpdateOrderAsync(model);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateOrderAsync_ShouldUpdateOrderWithTrimmedValues_WhenStringsAreProvided()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using (var dbContextSetup = GetDbContext(dbName))
            {
                dbContextSetup.OrderStatus.Add(new OrderStatus { Id = 1, Name = "New" });
                dbContextSetup.OrderStatus.Add(new OrderStatus { Id = 2, Name = "Shipped" });

                dbContextSetup.Order.Add(new Order
                {
                    Id = 1,
                    CustomerId = "u",
                    CustomerName = "n",
                    CustomerEmail = "e",
                    CustomerPhoneNumber = "1",
                    Street = "s",
                    PostalCode = "p",
                    City = "c",
                    PaymentMethodId = 1,
                    OrderStatusId = 1,
                    TotalAmount = 10
                });
                dbContextSetup.SaveChanges();
            }

            using var dbContext = GetDbContext(dbName);
            var service = new AdminOrderService(dbContext);

            var model = new AdminOrderDetailsVM
            {
                OrderId = 1,
                OrderStatusId = 2,
                Carrier = " DHL ",
                ShippingMethod = " Courier ",
                TrackingNumber = " 12345 ",
                ShippedDate = new DateTime(2023, 1, 1),
                ShippingNotes = " Note "
            };

            // Act
            var result = await service.UpdateOrderAsync(model);

            // Assert
            Assert.True(result);
            var updatedOrder = await dbContext.Order.FirstAsync(x => x.Id == 1);
            Assert.Equal(2, updatedOrder.OrderStatusId);
            Assert.Equal("DHL", updatedOrder.Carrier);
            Assert.Equal("Courier", updatedOrder.ShippingMethod);
            Assert.Equal("12345", updatedOrder.TrackingNumber);
            Assert.Equal(new DateTime(2023, 1, 1), updatedOrder.ShippedDate);
            Assert.Equal("Note", updatedOrder.ShippingNotes);
        }

        [Fact]
        public async Task UpdateOrderAsync_ShouldUpdateOrderWithNullValues_WhenStringsAreEmpty()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using (var dbContextSetup = GetDbContext(dbName))
            {
                dbContextSetup.OrderStatus.Add(new OrderStatus { Id = 1, Name = "New" });

                dbContextSetup.Order.Add(new Order
                {
                    Id = 1,
                    CustomerId = "u",
                    CustomerName = "n",
                    CustomerEmail = "e",
                    CustomerPhoneNumber = "1",
                    Street = "s",
                    PostalCode = "p",
                    City = "c",
                    PaymentMethodId = 1,
                    OrderStatusId = 1,
                    TotalAmount = 10
                });
                dbContextSetup.SaveChanges();
            }

            using var dbContext = GetDbContext(dbName);
            var service = new AdminOrderService(dbContext);

            var model = new AdminOrderDetailsVM
            {
                OrderId = 1,
                OrderStatusId = 1,
                Carrier = "",
                ShippingMethod = "   ",
                TrackingNumber = null,
                ShippingNotes = string.Empty
            };

            // Act
            var result = await service.UpdateOrderAsync(model);

            // Assert
            Assert.True(result);
            var updatedOrder = await dbContext.Order.FirstAsync(x => x.Id == 1);
            Assert.Equal(1, updatedOrder.OrderStatusId);
            Assert.Null(updatedOrder.Carrier);
            Assert.Null(updatedOrder.ShippingMethod);
            Assert.Null(updatedOrder.TrackingNumber);
            Assert.Null(updatedOrder.ShippingNotes);
        }
    }
}
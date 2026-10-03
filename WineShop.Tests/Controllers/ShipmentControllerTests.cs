using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Xunit;
using WineShop.Controllers;
using WineShop.Data;
using WineShop.Models;
using WineShop.Models.ViewModels;
using WineShop.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WineShop.Tests.Controllers
{
    public class ShipmentControllerTests
    {
        private ApplicationDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Index_ShouldReturnAllOrders_WhenUserIsAdmin()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using (var db = GetDbContext(dbName))
            {
                db.OrderStatus.Add(new OrderStatus { Id = 1, Name = "Shipped" });
                db.PaymentMethod.Add(new PaymentMethod { Id = 1, Name = "Credit Card" });

                db.Order.Add(new Order
                {
                    Id = 1,
                    CustomerId = "admin-id",
                    CustomerName = "Admin User",
                    CustomerEmail = "admin@test.com",
                    CustomerPhoneNumber = "111222333",
                    Street = "Test St",
                    PostalCode = "00-000",
                    City = "Test City",
                    PaymentMethodId = 1,
                    OrderStatusId = 1,
                    TotalAmount = 100
                });
                db.Order.Add(new Order
                {
                    Id = 2,
                    CustomerId = "customer-id",
                    CustomerName = "Customer User",
                    CustomerEmail = "customer@test.com",
                    CustomerPhoneNumber = "444555666",
                    Street = "Test St",
                    PostalCode = "00-000",
                    City = "Test City",
                    PaymentMethodId = 1,
                    OrderStatusId = 1,
                    TotalAmount = 200
                });
                db.SaveChanges();
            }

            using (var db = GetDbContext(dbName))
            {
                var controller = new ShipmentController(db);
                var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "admin-id"),
                    new Claim(ClaimTypes.Role, WC.AdminRole)
                }));
                controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

                // Act
                var result = await controller.Index();

                // Assert
                var viewResult = Assert.IsType<ViewResult>(result);
                var model = Assert.IsAssignableFrom<IEnumerable<ShipmentListItemVM>>(viewResult.Model);
                Assert.Equal(2, model.Count());
            }
        }

        [Fact]
        public async Task Index_ShouldReturnOnlyUserOrders_WhenUserIsNotAdmin()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using (var db = GetDbContext(dbName))
            {
                db.OrderStatus.Add(new OrderStatus { Id = 1, Name = "Shipped" });
                db.PaymentMethod.Add(new PaymentMethod { Id = 1, Name = "Credit Card" });

                db.Order.Add(new Order
                {
                    Id = 1,
                    CustomerId = "other-customer-id",
                    CustomerName = "Other User",
                    CustomerEmail = "other@test.com",
                    CustomerPhoneNumber = "111222333",
                    Street = "Test St",
                    PostalCode = "00-000",
                    City = "Test City",
                    PaymentMethodId = 1,
                    OrderStatusId = 1,
                    TotalAmount = 100
                });
                db.Order.Add(new Order
                {
                    Id = 2,
                    CustomerId = "my-customer-id",
                    CustomerName = "My User",
                    CustomerEmail = "my@test.com",
                    CustomerPhoneNumber = "444555666",
                    Street = "Test St",
                    PostalCode = "00-000",
                    City = "Test City",
                    PaymentMethodId = 1,
                    OrderStatusId = 1,
                    TotalAmount = 200
                });
                db.SaveChanges();
            }

            using (var db = GetDbContext(dbName))
            {
                var controller = new ShipmentController(db);
                var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "my-customer-id")
                }));
                controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

                // Act
                var result = await controller.Index();

                // Assert
                var viewResult = Assert.IsType<ViewResult>(result);
                var model = Assert.IsAssignableFrom<IEnumerable<ShipmentListItemVM>>(viewResult.Model);
                Assert.Single(model);
                Assert.Equal(2, model.First().OrderId);
            }
        }
    }
}
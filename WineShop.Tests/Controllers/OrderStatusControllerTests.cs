using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using WineShop.Controllers;
using WineShop.Data;
using WineShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WineShop.Tests.Controllers
{
    public class OrderStatusControllerTests
    {
        private ApplicationDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public void Index_ShouldReturnViewWithOrderStatuses()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using (var db = GetDbContext(dbName))
            {
                db.OrderStatus.Add(new OrderStatus { Id = 1, Name = "Pending" });
                db.OrderStatus.Add(new OrderStatus { Id = 2, Name = "Shipped" });
                db.SaveChanges();
            }

            using (var db = GetDbContext(dbName))
            {
                var controller = new OrderStatusController(db);

                // Act
                var result = controller.Index();

                // Assert
                var viewResult = Assert.IsType<ViewResult>(result);
                var model = Assert.IsAssignableFrom<IEnumerable<OrderStatus>>(viewResult.Model);
                Assert.Equal(2, model.Count());
            }
        }

        [Fact]
        public void CreateGet_ShouldReturnView()
        {
            // Arrange
            using var db = GetDbContext(Guid.NewGuid().ToString());
            var controller = new OrderStatusController(db);

            // Act
            var result = controller.Create();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void CreatePost_ShouldReturnView_WhenModelStateIsInvalid()
        {
            // Arrange
            using var db = GetDbContext(Guid.NewGuid().ToString());
            var controller = new OrderStatusController(db);
            controller.ModelState.AddModelError("Name", "Name is required");
            var newOrderStatus = new OrderStatus();

            // Act
            var result = controller.Create(newOrderStatus);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<OrderStatus>(viewResult.Model);
        }

        [Fact]
        public void CreatePost_ShouldAddOrderStatusAndRedirect_WhenValid()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var db = GetDbContext(dbName);
            var controller = new OrderStatusController(db);
            var newOrderStatus = new OrderStatus { Id = 1, Name = "Delivered" };

            // Act
            var result = controller.Create(newOrderStatus);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal(1, db.OrderStatus.Count());
            Assert.Equal("Delivered", db.OrderStatus.First().Name);
        }

        [Fact]
        public void EditGet_ShouldReturnNotFound_WhenIdIsNull()
        {
            // Arrange
            using var db = GetDbContext(Guid.NewGuid().ToString());
            var controller = new OrderStatusController(db);

            // Act
            var result1 = controller.Edit(0);
            var result2 = controller.Edit((int?)null);

            // Assert
            Assert.IsType<NotFoundResult>(result1);
            Assert.IsType<NotFoundResult>(result2);
        }

        [Fact]
        public void EditGet_ShouldReturnNotFound_WhenOrderStatusDoesNotExist()
        {
            // Arrange
            using var db = GetDbContext(Guid.NewGuid().ToString());
            var controller = new OrderStatusController(db);

            // Act
            var result = controller.Edit(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void EditGet_ShouldReturnViewWithModel_WhenOrderStatusExists()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using (var db = GetDbContext(dbName))
            {
                db.OrderStatus.Add(new OrderStatus { Id = 5, Name = "Processing" });
                db.SaveChanges();
            }

            using (var db = GetDbContext(dbName))
            {
                var controller = new OrderStatusController(db);

                // Act
                var result = controller.Edit(5);

                // Assert
                var viewResult = Assert.IsType<ViewResult>(result);
                var model = Assert.IsType<OrderStatus>(viewResult.Model);
                Assert.Equal(5, model.Id);
            }
        }

        [Fact]
        public void EditPost_ShouldReturnView_WhenModelStateIsInvalid()
        {
            // Arrange
            using var db = GetDbContext(Guid.NewGuid().ToString());
            var controller = new OrderStatusController(db);
            controller.ModelState.AddModelError("Name", "Name is required");
            var orderStatusToUpdate = new OrderStatus { Id = 1 };

            // Act
            var result = controller.Edit(orderStatusToUpdate);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<OrderStatus>(viewResult.Model);
        }

        [Fact]
        public void EditPost_ShouldUpdateOrderStatusAndRedirect_WhenValid()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using (var db = GetDbContext(dbName))
            {
                db.OrderStatus.Add(new OrderStatus { Id = 1, Name = "Old Status" });
                db.SaveChanges();
            }

            using (var db = GetDbContext(dbName))
            {
                var controller = new OrderStatusController(db);
                var updatedOrderStatus = new OrderStatus { Id = 1, Name = "New Status" };

                // Act
                var result = controller.Edit(updatedOrderStatus);

                // Assert
                var redirectResult = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal("Index", redirectResult.ActionName);

                var inDb = db.OrderStatus.First(m => m.Id == 1);
                Assert.Equal("New Status", inDb.Name);
            }
        }

        [Fact]
        public void DeleteGet_ShouldReturnNotFound_WhenIdIsNull()
        {
            // Arrange
            using var db = GetDbContext(Guid.NewGuid().ToString());
            var controller = new OrderStatusController(db);

            // Act
            var result1 = controller.Delete(0);
            var result2 = controller.Delete((int?)null);

            // Assert
            Assert.IsType<NotFoundResult>(result1);
            Assert.IsType<NotFoundResult>(result2);
        }

        [Fact]
        public void DeleteGet_ShouldReturnNotFound_WhenOrderStatusDoesNotExist()
        {
            // Arrange
            using var db = GetDbContext(Guid.NewGuid().ToString());
            var controller = new OrderStatusController(db);

            // Act
            var result = controller.Delete(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void DeleteGet_ShouldReturnViewWithModel_WhenOrderStatusExists()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using (var db = GetDbContext(dbName))
            {
                db.OrderStatus.Add(new OrderStatus { Id = 3, Name = "Status To Delete" });
                db.SaveChanges();
            }

            using (var db = GetDbContext(dbName))
            {
                var controller = new OrderStatusController(db);

                // Act
                var result = controller.Delete(3);

                // Assert
                var viewResult = Assert.IsType<ViewResult>(result);
                var model = Assert.IsType<OrderStatus>(viewResult.Model);
                Assert.Equal(3, model.Id);
            }
        }

        [Fact]
        public void DeletePost_ShouldReturnNotFound_WhenOrderStatusDoesNotExist()
        {
            // Arrange
            using var db = GetDbContext(Guid.NewGuid().ToString());
            var controller = new OrderStatusController(db);

            // Act
            var result = controller.DeletePost(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void DeletePost_ShouldRemoveOrderStatusAndRedirect_WhenValid()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using (var db = GetDbContext(dbName))
            {
                db.OrderStatus.Add(new OrderStatus { Id = 7, Name = "Bad Status" });
                db.SaveChanges();
            }

            using (var db = GetDbContext(dbName))
            {
                var controller = new OrderStatusController(db);

                // Act
                var result = controller.DeletePost(7);

                // Assert
                var redirectResult = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal("Index", redirectResult.ActionName);
                Assert.Empty(db.OrderStatus);
            }
        }
    }
}
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
    public class PaymentMethodControllerTests
    {
        private ApplicationDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public void Index_ShouldReturnViewWithPaymentMethods()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using (var db = GetDbContext(dbName))
            {
                db.PaymentMethod.Add(new PaymentMethod { Id = 1, Name = "Credit Card" });
                db.PaymentMethod.Add(new PaymentMethod { Id = 2, Name = "PayPal" });
                db.SaveChanges();
            }

            using (var db = GetDbContext(dbName))
            {
                var controller = new PaymentMethodController(db);

                // Act
                var result = controller.Index();

                // Assert
                var viewResult = Assert.IsType<ViewResult>(result);
                var model = Assert.IsAssignableFrom<IEnumerable<PaymentMethod>>(viewResult.Model);
                Assert.Equal(2, model.Count());
            }
        }

        [Fact]
        public void CreateGet_ShouldReturnView()
        {
            // Arrange
            using var db = GetDbContext(Guid.NewGuid().ToString());
            var controller = new PaymentMethodController(db);

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
            var controller = new PaymentMethodController(db);
            controller.ModelState.AddModelError("Name", "Name is required");
            var newPaymentMethod = new PaymentMethod();

            // Act
            var result = controller.Create(newPaymentMethod);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<PaymentMethod>(viewResult.Model);
        }

        [Fact]
        public void CreatePost_ShouldAddPaymentMethodAndRedirect_WhenValid()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var db = GetDbContext(dbName);
            var controller = new PaymentMethodController(db);
            var newPaymentMethod = new PaymentMethod { Id = 1, Name = "Bank Transfer" };

            // Act
            var result = controller.Create(newPaymentMethod);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal(1, db.PaymentMethod.Count());
            Assert.Equal("Bank Transfer", db.PaymentMethod.First().Name);
        }

        [Fact]
        public void EditGet_ShouldReturnNotFound_WhenIdIsNull()
        {
            // Arrange
            using var db = GetDbContext(Guid.NewGuid().ToString());
            var controller = new PaymentMethodController(db);

            // Act
            var result1 = controller.Edit(0);
            var result2 = controller.Edit((int?)null);

            // Assert
            Assert.IsType<NotFoundResult>(result1);
            Assert.IsType<NotFoundResult>(result2);
        }

        [Fact]
        public void EditGet_ShouldReturnNotFound_WhenPaymentMethodDoesNotExist()
        {
            // Arrange
            using var db = GetDbContext(Guid.NewGuid().ToString());
            var controller = new PaymentMethodController(db);

            // Act
            var result = controller.Edit(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void EditGet_ShouldReturnViewWithModel_WhenPaymentMethodExists()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using (var db = GetDbContext(dbName))
            {
                db.PaymentMethod.Add(new PaymentMethod { Id = 5, Name = "Cash" });
                db.SaveChanges();
            }

            using (var db = GetDbContext(dbName))
            {
                var controller = new PaymentMethodController(db);

                // Act
                var result = controller.Edit(5);

                // Assert
                var viewResult = Assert.IsType<ViewResult>(result);
                var model = Assert.IsType<PaymentMethod>(viewResult.Model);
                Assert.Equal(5, model.Id);
            }
        }

        [Fact]
        public void EditPost_ShouldReturnView_WhenModelStateIsInvalid()
        {
            // Arrange
            using var db = GetDbContext(Guid.NewGuid().ToString());
            var controller = new PaymentMethodController(db);
            controller.ModelState.AddModelError("Name", "Name is required");
            var paymentMethodToUpdate = new PaymentMethod { Id = 1 };

            // Act
            var result = controller.Edit(paymentMethodToUpdate);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<PaymentMethod>(viewResult.Model);
        }

        [Fact]
        public void EditPost_ShouldUpdatePaymentMethodAndRedirect_WhenValid()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using (var db = GetDbContext(dbName))
            {
                db.PaymentMethod.Add(new PaymentMethod { Id = 1, Name = "Old Method" });
                db.SaveChanges();
            }

            using (var db = GetDbContext(dbName))
            {
                var controller = new PaymentMethodController(db);
                var updatedPaymentMethod = new PaymentMethod { Id = 1, Name = "New Method" };

                // Act
                var result = controller.Edit(updatedPaymentMethod);

                // Assert
                var redirectResult = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal("Index", redirectResult.ActionName);

                var inDb = db.PaymentMethod.First(m => m.Id == 1);
                Assert.Equal("New Method", inDb.Name);
            }
        }

        [Fact]
        public void DeleteGet_ShouldReturnNotFound_WhenIdIsNull()
        {
            // Arrange
            using var db = GetDbContext(Guid.NewGuid().ToString());
            var controller = new PaymentMethodController(db);

            // Act
            var result1 = controller.Delete(0);
            var result2 = controller.Delete((int?)null);

            // Assert
            Assert.IsType<NotFoundResult>(result1);
            Assert.IsType<NotFoundResult>(result2);
        }

        [Fact]
        public void DeleteGet_ShouldReturnNotFound_WhenPaymentMethodDoesNotExist()
        {
            // Arrange
            using var db = GetDbContext(Guid.NewGuid().ToString());
            var controller = new PaymentMethodController(db);

            // Act
            var result = controller.Delete(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void DeleteGet_ShouldReturnViewWithModel_WhenPaymentMethodExists()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using (var db = GetDbContext(dbName))
            {
                db.PaymentMethod.Add(new PaymentMethod { Id = 3, Name = "Method To Delete" });
                db.SaveChanges();
            }

            using (var db = GetDbContext(dbName))
            {
                var controller = new PaymentMethodController(db);

                // Act
                var result = controller.Delete(3);

                // Assert
                var viewResult = Assert.IsType<ViewResult>(result);
                var model = Assert.IsType<PaymentMethod>(viewResult.Model);
                Assert.Equal(3, model.Id);
            }
        }

        [Fact]
        public void DeletePost_ShouldReturnNotFound_WhenPaymentMethodDoesNotExist()
        {
            // Arrange
            using var db = GetDbContext(Guid.NewGuid().ToString());
            var controller = new PaymentMethodController(db);

            // Act
            var result = controller.DeletePost(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void DeletePost_ShouldRemovePaymentMethodAndRedirect_WhenValid()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using (var db = GetDbContext(dbName))
            {
                db.PaymentMethod.Add(new PaymentMethod { Id = 7, Name = "Bad Method" });
                db.SaveChanges();
            }

            using (var db = GetDbContext(dbName))
            {
                var controller = new PaymentMethodController(db);

                // Act
                var result = controller.DeletePost(7);

                // Assert
                var redirectResult = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal("Index", redirectResult.ActionName);
                Assert.Empty(db.PaymentMethod);
            }
        }
    }
}
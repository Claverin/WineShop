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
    public class ProductTypeControllerTests
    {
        private ApplicationDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public void Index_ShouldReturnViewWithProductTypes()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using (var db = GetDbContext(dbName))
            {
                db.ProductType.Add(new ProductType { Id = 1, Name = "Red Wine" });
                db.ProductType.Add(new ProductType { Id = 2, Name = "White Wine" });
                db.SaveChanges();
            }

            using (var db = GetDbContext(dbName))
            {
                var controller = new ProductTypeController(db);

                // Act
                var result = controller.Index();

                // Assert
                var viewResult = Assert.IsType<ViewResult>(result);
                var model = Assert.IsAssignableFrom<IEnumerable<ProductType>>(viewResult.Model);
                Assert.Equal(2, model.Count());
            }
        }

        [Fact]
        public void CreateGet_ShouldReturnView()
        {
            // Arrange
            using var db = GetDbContext(Guid.NewGuid().ToString());
            var controller = new ProductTypeController(db);

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
            var controller = new ProductTypeController(db);
            controller.ModelState.AddModelError("Name", "Name is required");
            var newProductType = new ProductType();

            // Act
            var result = controller.Create(newProductType);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<ProductType>(viewResult.Model);
        }

        [Fact]
        public void CreatePost_ShouldAddProductTypeAndRedirect_WhenValid()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var db = GetDbContext(dbName);
            var controller = new ProductTypeController(db);
            var newProductType = new ProductType { Id = 1, Name = "Rose Wine" };

            // Act
            var result = controller.Create(newProductType);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal(1, db.ProductType.Count());
            Assert.Equal("Rose Wine", db.ProductType.First().Name);
        }

        [Fact]
        public void EditGet_ShouldReturnNotFound_WhenIdIsNull()
        {
            // Arrange
            using var db = GetDbContext(Guid.NewGuid().ToString());
            var controller = new ProductTypeController(db);

            // Act
            var result1 = controller.Edit(0);
            var result2 = controller.Edit((int?)null);

            // Assert
            Assert.IsType<NotFoundResult>(result1);
            Assert.IsType<NotFoundResult>(result2);
        }

        [Fact]
        public void EditGet_ShouldReturnNotFound_WhenProductTypeDoesNotExist()
        {
            // Arrange
            using var db = GetDbContext(Guid.NewGuid().ToString());
            var controller = new ProductTypeController(db);

            // Act
            var result = controller.Edit(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void EditGet_ShouldReturnViewWithModel_WhenProductTypeExists()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using (var db = GetDbContext(dbName))
            {
                db.ProductType.Add(new ProductType { Id = 5, Name = "Sparkling Wine" });
                db.SaveChanges();
            }

            using (var db = GetDbContext(dbName))
            {
                var controller = new ProductTypeController(db);

                // Act
                var result = controller.Edit(5);

                // Assert
                var viewResult = Assert.IsType<ViewResult>(result);
                var model = Assert.IsType<ProductType>(viewResult.Model);
                Assert.Equal(5, model.Id);
            }
        }

        [Fact]
        public void EditPost_ShouldReturnView_WhenModelStateIsInvalid()
        {
            // Arrange
            using var db = GetDbContext(Guid.NewGuid().ToString());
            var controller = new ProductTypeController(db);
            controller.ModelState.AddModelError("Name", "Name is required");
            var productTypeToUpdate = new ProductType { Id = 1 };

            // Act
            var result = controller.Edit(productTypeToUpdate);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<ProductType>(viewResult.Model);
        }

        [Fact]
        public void EditPost_ShouldUpdateProductTypeAndRedirect_WhenValid()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using (var db = GetDbContext(dbName))
            {
                db.ProductType.Add(new ProductType { Id = 1, Name = "Old Type" });
                db.SaveChanges();
            }

            using (var db = GetDbContext(dbName))
            {
                var controller = new ProductTypeController(db);
                var updatedProductType = new ProductType { Id = 1, Name = "New Type" };

                // Act
                var result = controller.Edit(updatedProductType);

                // Assert
                var redirectResult = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal("Index", redirectResult.ActionName);

                var inDb = db.ProductType.First(pt => pt.Id == 1);
                Assert.Equal("New Type", inDb.Name);
            }
        }

        [Fact]
        public void DeleteGet_ShouldReturnNotFound_WhenIdIsNull()
        {
            // Arrange
            using var db = GetDbContext(Guid.NewGuid().ToString());
            var controller = new ProductTypeController(db);

            // Act
            var result1 = controller.Delete(0);
            var result2 = controller.Delete((int?)null);

            // Assert
            Assert.IsType<NotFoundResult>(result1);
            Assert.IsType<NotFoundResult>(result2);
        }

        [Fact]
        public void DeleteGet_ShouldReturnNotFound_WhenProductTypeDoesNotExist()
        {
            // Arrange
            using var db = GetDbContext(Guid.NewGuid().ToString());
            var controller = new ProductTypeController(db);

            // Act
            var result = controller.Delete(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void DeleteGet_ShouldReturnViewWithModel_WhenProductTypeExists()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using (var db = GetDbContext(dbName))
            {
                db.ProductType.Add(new ProductType { Id = 3, Name = "Type To Delete" });
                db.SaveChanges();
            }

            using (var db = GetDbContext(dbName))
            {
                var controller = new ProductTypeController(db);

                // Act
                var result = controller.Delete(3);

                // Assert
                var viewResult = Assert.IsType<ViewResult>(result);
                var model = Assert.IsType<ProductType>(viewResult.Model);
                Assert.Equal(3, model.Id);
            }
        }

        [Fact]
        public void DeletePost_ShouldReturnNotFound_WhenProductTypeDoesNotExist()
        {
            // Arrange
            using var db = GetDbContext(Guid.NewGuid().ToString());
            var controller = new ProductTypeController(db);

            // Act
            var result = controller.DeletePost(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void DeletePost_ShouldRemoveProductTypeAndRedirect_WhenValid()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using (var db = GetDbContext(dbName))
            {
                db.ProductType.Add(new ProductType { Id = 7, Name = "Bad Type" });
                db.SaveChanges();
            }

            using (var db = GetDbContext(dbName))
            {
                var controller = new ProductTypeController(db);

                // Act
                var result = controller.DeletePost(7);

                // Assert
                var redirectResult = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal("Index", redirectResult.ActionName);
                Assert.Empty(db.ProductType);
            }
        }
    }
}
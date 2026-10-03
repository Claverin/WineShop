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
    public class ManufacturerControllerTests
    {
        private ApplicationDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public void Index_ShouldReturnViewWithManufacturers()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using (var db = GetDbContext(dbName))
            {
                db.Manufacturer.Add(new Manufacturer { Id = 1, Name = "Winery A", Country = "Poland" });
                db.Manufacturer.Add(new Manufacturer { Id = 2, Name = "Winery B", Country = "France" });
                db.SaveChanges();
            }

            using (var db = GetDbContext(dbName))
            {
                var controller = new ManufacturerController(db);

                // Act
                var result = controller.Index();

                // Assert
                var viewResult = Assert.IsType<ViewResult>(result);
                var model = Assert.IsAssignableFrom<IEnumerable<Manufacturer>>(viewResult.Model);
                Assert.Equal(2, model.Count());
            }
        }

        [Fact]
        public void CreateGet_ShouldReturnView()
        {
            // Arrange
            using var db = GetDbContext(Guid.NewGuid().ToString());
            var controller = new ManufacturerController(db);

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
            var controller = new ManufacturerController(db);
            controller.ModelState.AddModelError("Name", "Name is required");
            var newManufacturer = new Manufacturer();

            // Act
            var result = controller.Create(newManufacturer);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<Manufacturer>(viewResult.Model);
        }

        [Fact]
        public void CreatePost_ShouldAddManufacturerAndRedirect_WhenValid()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var db = GetDbContext(dbName);
            var controller = new ManufacturerController(db);
            var newManufacturer = new Manufacturer { Id = 1, Name = "New Winery", Country = "Italy" };

            // Act
            var result = controller.Create(newManufacturer);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal(1, db.Manufacturer.Count());
            Assert.Equal("New Winery", db.Manufacturer.First().Name);
        }

        [Fact]
        public void EditGet_ShouldReturnNotFound_WhenIdIsNull()
        {
            // Arrange
            using var db = GetDbContext(Guid.NewGuid().ToString());
            var controller = new ManufacturerController(db);

            // Act
            var result1 = controller.Edit(0);
            var result2 = controller.Edit((int?)null);

            // Assert
            Assert.IsType<NotFoundResult>(result1);
            Assert.IsType<NotFoundResult>(result2);
        }

        [Fact]
        public void EditGet_ShouldReturnNotFound_WhenManufacturerDoesNotExist()
        {
            // Arrange
            using var db = GetDbContext(Guid.NewGuid().ToString());
            var controller = new ManufacturerController(db);

            // Act
            var result = controller.Edit(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void EditGet_ShouldReturnViewWithModel_WhenManufacturerExists()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using (var db = GetDbContext(dbName))
            {
                db.Manufacturer.Add(new Manufacturer { Id = 5, Name = "Old Winery", Country = "Spain" });
                db.SaveChanges();
            }

            using (var db = GetDbContext(dbName))
            {
                var controller = new ManufacturerController(db);

                // Act
                var result = controller.Edit(5);

                // Assert
                var viewResult = Assert.IsType<ViewResult>(result);
                var model = Assert.IsType<Manufacturer>(viewResult.Model);
                Assert.Equal(5, model.Id);
            }
        }

        [Fact]
        public void EditPost_ShouldReturnView_WhenModelStateIsInvalid()
        {
            // Arrange
            using var db = GetDbContext(Guid.NewGuid().ToString());
            var controller = new ManufacturerController(db);
            controller.ModelState.AddModelError("Name", "Name is required");
            var manufacturerToUpdate = new Manufacturer { Id = 1 };

            // Act
            var result = controller.Edit(manufacturerToUpdate);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<Manufacturer>(viewResult.Model);
        }

        [Fact]
        public void EditPost_ShouldUpdateManufacturerAndRedirect_WhenValid()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using (var db = GetDbContext(dbName))
            {
                db.Manufacturer.Add(new Manufacturer { Id = 1, Name = "Old Name", Country = "Germany" });
                db.SaveChanges();
            }

            using (var db = GetDbContext(dbName))
            {
                var controller = new ManufacturerController(db);
                var updatedManufacturer = new Manufacturer { Id = 1, Name = "New Name", Country = "Germany" };

                // Act
                var result = controller.Edit(updatedManufacturer);

                // Assert
                var redirectResult = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal("Index", redirectResult.ActionName);

                var inDb = db.Manufacturer.First(m => m.Id == 1);
                Assert.Equal("New Name", inDb.Name);
            }
        }

        [Fact]
        public void DeleteGet_ShouldReturnNotFound_WhenIdIsNull()
        {
            // Arrange
            using var db = GetDbContext(Guid.NewGuid().ToString());
            var controller = new ManufacturerController(db);

            // Act
            var result1 = controller.Delete(0);
            var result2 = controller.Delete((int?)null);

            // Assert
            Assert.IsType<NotFoundResult>(result1);
            Assert.IsType<NotFoundResult>(result2);
        }

        [Fact]
        public void DeleteGet_ShouldReturnNotFound_WhenManufacturerDoesNotExist()
        {
            // Arrange
            using var db = GetDbContext(Guid.NewGuid().ToString());
            var controller = new ManufacturerController(db);

            // Act
            var result = controller.Delete(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void DeleteGet_ShouldReturnViewWithModel_WhenManufacturerExists()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using (var db = GetDbContext(dbName))
            {
                db.Manufacturer.Add(new Manufacturer { Id = 3, Name = "Winery To Delete", Country = "Portugal" });
                db.SaveChanges();
            }

            using (var db = GetDbContext(dbName))
            {
                var controller = new ManufacturerController(db);

                // Act
                var result = controller.Delete(3);

                // Assert
                var viewResult = Assert.IsType<ViewResult>(result);
                var model = Assert.IsType<Manufacturer>(viewResult.Model);
                Assert.Equal(3, model.Id);
            }
        }

        [Fact]
        public void DeletePost_ShouldReturnNotFound_WhenManufacturerDoesNotExist()
        {
            // Arrange
            using var db = GetDbContext(Guid.NewGuid().ToString());
            var controller = new ManufacturerController(db);

            // Act
            var result = controller.DeletePost(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void DeletePost_ShouldRemoveManufacturerAndRedirect_WhenValid()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using (var db = GetDbContext(dbName))
            {
                db.Manufacturer.Add(new Manufacturer { Id = 7, Name = "Bad Winery", Country = "Chile" });
                db.SaveChanges();
            }

            using (var db = GetDbContext(dbName))
            {
                var controller = new ManufacturerController(db);

                // Act
                var result = controller.DeletePost(7);

                // Assert
                var redirectResult = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal("Index", redirectResult.ActionName);
                Assert.Empty(db.Manufacturer);
            }
        }
    }
}
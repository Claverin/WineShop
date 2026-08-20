using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using WineShop.Controllers;
using WineShop.Data;
using WineShop.Models;
using WineShop.Models.ViewModels;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

namespace WineShop.Tests.Controllers
{
    public class ProductControllerTests
    {
        private ApplicationDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public void Index_ShouldReturnViewWithProducts()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using (var db = GetDbContext(dbName))
            {
                db.Manufacturer.Add(new Manufacturer { Id = 1, Name = "Winnica Test", Country = "Polska" });
                db.ProductType.Add(new ProductType { Id = 1, Name = "Czerwone" });
                db.Product.Add(new Product { Id = 1, Name = "Wino 1", Price = 50, Image = "test.png", IdManufacturer = 1, IdProductType = 1 });
                db.SaveChanges();
            }

            using (var db = GetDbContext(dbName))
            {
                var webHostEnvMock = new Mock<IWebHostEnvironment>();
                var controller = new ProductController(db, webHostEnvMock.Object);

                // Act
                var result = controller.Index();

                // Assert
                var viewResult = Assert.IsType<ViewResult>(result);
                var model = Assert.IsAssignableFrom<IEnumerable<Product>>(viewResult.Model);
                Assert.Single(model);
            }
        }

        [Fact]
        public void CreateGet_ShouldReturnViewWithProductVM()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using (var db = GetDbContext(dbName))
            {
                db.Manufacturer.Add(new Manufacturer { Id = 1, Name = "M1", Country = "Polska" });
                db.ProductType.Add(new ProductType { Id = 1, Name = "T1" });
                db.SaveChanges();
            }

            using (var db = GetDbContext(dbName))
            {
                var webHostEnvMock = new Mock<IWebHostEnvironment>();
                var controller = new ProductController(db, webHostEnvMock.Object);

                // Act
                var result = controller.Create();

                // Assert
                var viewResult = Assert.IsType<ViewResult>(result);
                var model = Assert.IsType<ProductVM>(viewResult.Model);
                Assert.NotNull(model.Product);
                Assert.Single(model.ManufacturerSelectList);
                Assert.Single(model.ProductTypeSelectList);
            }
        }

        [Fact]
        public void CreatePost_ShouldReturnView_WhenModelStateIsInvalid()
        {
            // Arrange
            var db = GetDbContext(Guid.NewGuid().ToString());
            var webHostEnvMock = new Mock<IWebHostEnvironment>();
            var controller = new ProductController(db, webHostEnvMock.Object);

            controller.ModelState.AddModelError("Name", "Name is required");
            var newProduct = new Product();

            // Act
            var result = controller.Create(newProduct);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<ProductVM>(viewResult.Model);
        }

        [Fact]
        public void CreatePost_ShouldAddErrorAndReturnView_WhenFileExtensionIsInvalid()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using (var db = GetDbContext(dbName))
            {
                db.Manufacturer.Add(new Manufacturer { Id = 1, Name = "M1", Country = "PL" });
                db.ProductType.Add(new ProductType { Id = 1, Name = "T1" });
                db.SaveChanges();
            }

            using (var db = GetDbContext(dbName))
            {
                var webHostEnvMock = new Mock<IWebHostEnvironment>();
                var controller = new ProductController(db, webHostEnvMock.Object);

                var fileMock = new Mock<IFormFile>();
                fileMock.Setup(f => f.FileName).Returns("dokument.txt");

                var fileCollectionMock = new Mock<IFormFileCollection>();
                fileCollectionMock.Setup(f => f[0]).Returns(fileMock.Object);
                fileCollectionMock.Setup(f => f.Count).Returns(1);

                var formMock = new Mock<IFormCollection>();
                formMock.Setup(f => f.Files).Returns(fileCollectionMock.Object);

                var httpContext = new DefaultHttpContext();
                httpContext.Request.Form = formMock.Object;
                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

                var newProduct = new Product { Name = "Test", Price = 10, IdManufacturer = 1, IdProductType = 1 };

                // Act
                var result = controller.Create(newProduct);

                // Assert
                var viewResult = Assert.IsType<ViewResult>(result);
                Assert.IsType<ProductVM>(viewResult.Model);
                Assert.True(controller.ModelState.ContainsKey("Image"));
            }
        }

        [Fact]
        public void CreatePost_ShouldAddProductAndRedirect_WhenValidAndFileIsImage()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var db = GetDbContext(dbName);

            var webHostEnvMock = new Mock<IWebHostEnvironment>();
            string tempWebRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempWebRoot);
            webHostEnvMock.Setup(w => w.WebRootPath).Returns(tempWebRoot);

            string expectedUploadPath = $"{tempWebRoot}{WC.ImageProductPath}";
            Directory.CreateDirectory(expectedUploadPath);

            var controller = new ProductController(db, webHostEnvMock.Object);

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("obrazek.png");
            fileMock.Setup(f => f.CopyTo(It.IsAny<Stream>())).Callback<Stream>(s => { });

            var fileCollectionMock = new Mock<IFormFileCollection>();
            fileCollectionMock.Setup(f => f[0]).Returns(fileMock.Object);
            fileCollectionMock.Setup(f => f.Count).Returns(1);

            var formMock = new Mock<IFormCollection>();
            formMock.Setup(f => f.Files).Returns(fileCollectionMock.Object);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Form = formMock.Object;
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var newProduct = new Product { Id = 1, Name = "Nowe Wino", Price = 99, IdManufacturer = 1, IdProductType = 1 };

            // Act
            var result = controller.Create(newProduct);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal(1, db.Product.Count());
            Assert.Contains(".png", db.Product.First().Image);
        }

        [Fact]
        public void EditGet_ShouldReturnNull_WhenIdIsZeroOrNull()
        {
            // Arrange
            var db = GetDbContext(Guid.NewGuid().ToString());
            var controller = new ProductController(db, null);

            // Act
            var result1 = controller.Edit(0);
            var result2 = controller.Edit((int?)null);

            // Assert
            Assert.Null(result1);
            Assert.Null(result2);
        }

        [Fact]
        public void EditGet_ShouldReturnNull_WhenProductDoesNotExist()
        {
            // Arrange
            var db = GetDbContext(Guid.NewGuid().ToString());
            var controller = new ProductController(db, null);

            // Act
            var result = controller.Edit(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void EditGet_ShouldReturnViewWithModel_WhenProductExists()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using (var db = GetDbContext(dbName))
            {
                db.Manufacturer.Add(new Manufacturer { Id = 1, Name = "M1", Country = "PL" });
                db.ProductType.Add(new ProductType { Id = 1, Name = "T1" });
                db.Product.Add(new Product { Id = 99, Name = "Stare Wino", Price = 10, Image = "img.png", IdManufacturer = 1, IdProductType = 1 });
                db.SaveChanges();
            }

            using (var db = GetDbContext(dbName))
            {
                var controller = new ProductController(db, null);

                // Act
                var result = controller.Edit(99);

                // Assert
                var viewResult = Assert.IsType<ViewResult>(result);
                var model = Assert.IsType<ProductVM>(viewResult.Model);
                Assert.Equal(99, model.Product.Id);
            }
        }

        [Fact]
        public void EditPost_ShouldUpdateProductWithoutNewFile_WhenNoFileUploaded()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using (var db = GetDbContext(dbName))
            {
                var existingProduct = new Product { Id = 1, Name = "Stare", Price = 10, Image = "old.png", IdManufacturer = 1, IdProductType = 1 };
                db.Product.Add(existingProduct);
                db.SaveChanges();
            }

            using (var db = GetDbContext(dbName))
            {
                var controller = new ProductController(db, null);

                var fileCollectionMock = new Mock<IFormFileCollection>();
                fileCollectionMock.Setup(f => f.Count).Returns(0);

                var formMock = new Mock<IFormCollection>();
                formMock.Setup(f => f.Files).Returns(fileCollectionMock.Object);

                var httpContext = new DefaultHttpContext();
                httpContext.Request.Form = formMock.Object;
                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

                var productToUpdate = new Product { Id = 1, Name = "Nowe", Price = 20, IdManufacturer = 1, IdProductType = 1 };

                // Act
                var result = controller.Edit(productToUpdate);

                // Assert
                var redirectResult = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal("Index", redirectResult.ActionName);

                var updatedProduct = db.Product.FirstOrDefault(p => p.Id == 1);
                Assert.Equal("Nowe", updatedProduct.Name);
                Assert.Equal("old.png", updatedProduct.Image);
            }
        }

        [Fact]
        public void EditPost_ShouldDeleteOldFileAndReplace_WhenNewFileUploaded()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using (var db = GetDbContext(dbName))
            {
                db.Product.Add(new Product { Id = 1, Name = "Stare", Price = 10, Image = "old.png", IdManufacturer = 1, IdProductType = 1 });
                db.SaveChanges();
            }

            var webHostEnvMock = new Mock<IWebHostEnvironment>();
            string tempWebRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempWebRoot);
            webHostEnvMock.Setup(w => w.WebRootPath).Returns(tempWebRoot);

            string expectedUploadPath = $"{tempWebRoot}{WC.ImageProductPath}";
            Directory.CreateDirectory(expectedUploadPath);

            string oldFilePath = Path.Combine(expectedUploadPath, "old.png");
            File.WriteAllText(oldFilePath, "fake image content");

            using (var db = GetDbContext(dbName))
            {
                var controller = new ProductController(db, webHostEnvMock.Object);

                var fileMock = new Mock<IFormFile>();
                fileMock.Setup(f => f.FileName).Returns("new.png");
                fileMock.Setup(f => f.CopyTo(It.IsAny<Stream>())).Callback<Stream>(s => { });

                var fileCollectionMock = new Mock<IFormFileCollection>();
                fileCollectionMock.Setup(f => f[0]).Returns(fileMock.Object);
                fileCollectionMock.Setup(f => f.Count).Returns(1);

                var formMock = new Mock<IFormCollection>();
                formMock.Setup(f => f.Files).Returns(fileCollectionMock.Object);

                var httpContext = new DefaultHttpContext();
                httpContext.Request.Form = formMock.Object;
                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

                var productToUpdate = new Product { Id = 1, Name = "Nowe", Price = 20, IdManufacturer = 1, IdProductType = 1 };

                // Act
                var result = controller.Edit(productToUpdate);

                // Assert
                var redirectResult = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal("Index", redirectResult.ActionName);
                Assert.False(File.Exists(oldFilePath));
            }
        }

        [Fact]
        public void EditPost_ShouldAddErrorAndReturnView_WhenFileExtensionIsInvalid()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using (var db = GetDbContext(dbName))
            {
                db.Manufacturer.Add(new Manufacturer { Id = 1, Name = "M1", Country = "PL" });
                db.ProductType.Add(new ProductType { Id = 1, Name = "T1" });
                // KLUCZOWE: Musimy dodać ten produkt do bazy, bo kontroler robi return Edit(product.Id), 
                // który szuka produktu w bazie przez Find(id) i buduje dropdowny!
                db.Product.Add(new Product { Id = 1, Name = "Stare", Price = 10, Image = "old.png", IdManufacturer = 1, IdProductType = 1 });
                db.SaveChanges();
            }

            using (var db = GetDbContext(dbName))
            {
                var webHostEnvMock = new Mock<IWebHostEnvironment>();
                var controller = new ProductController(db, webHostEnvMock.Object);

                var fileMock = new Mock<IFormFile>();
                fileMock.Setup(f => f.FileName).Returns("dokument.txt");

                var fileCollectionMock = new Mock<IFormFileCollection>();
                fileCollectionMock.Setup(f => f[0]).Returns(fileMock.Object);
                fileCollectionMock.Setup(f => f.Count).Returns(1);

                var formMock = new Mock<IFormCollection>();
                formMock.Setup(f => f.Files).Returns(fileCollectionMock.Object);

                var httpContext = new DefaultHttpContext();
                httpContext.Request.Form = formMock.Object;
                controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

                var productToUpdate = new Product { Id = 1, Name = "Nowe", Price = 20, IdManufacturer = 1, IdProductType = 1 };

                // Act
                var result = controller.Edit(productToUpdate);

                // Assert
                var viewResult = Assert.IsType<ViewResult>(result);
                Assert.IsType<ProductVM>(viewResult.Model);
                Assert.True(controller.ModelState.ContainsKey("Image"));
            }
        }

        [Fact]
        public void EditPost_ShouldReturnView_WhenModelStateIsInvalid()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using (var db = GetDbContext(dbName))
            {
                db.Manufacturer.Add(new Manufacturer { Id = 1, Name = "M1", Country = "PL" });
                db.ProductType.Add(new ProductType { Id = 1, Name = "T1" });
                db.Product.Add(new Product { Id = 1, Name = "Test", Price = 10, Image = "img.png", IdManufacturer = 1, IdProductType = 1 });
                db.SaveChanges();
            }

            using (var db = GetDbContext(dbName))
            {
                var webHostEnvMock = new Mock<IWebHostEnvironment>();
                var controller = new ProductController(db, webHostEnvMock.Object);
                controller.ModelState.AddModelError("Name", "Required");

                var product = new Product { Id = 1 };

                // Act
                var result = controller.Edit(product);

                // Assert
                var viewResult = Assert.IsType<ViewResult>(result);
                Assert.IsType<ProductVM>(viewResult.Model);
            }
        }

        [Fact]
        public void DeleteGet_ShouldReturnNull_WhenIdIsZeroOrNull()
        {
            // Arrange
            var db = GetDbContext(Guid.NewGuid().ToString());
            var controller = new ProductController(db, null);

            // Act
            var result1 = controller.Delete(0);
            var result2 = controller.Delete((int?)null);

            // Assert
            Assert.Null(result1);
            Assert.Null(result2);
        }

        [Fact]
        public void DeleteGet_ShouldReturnNull_WhenProductDoesNotExist()
        {
            // Arrange
            var db = GetDbContext(Guid.NewGuid().ToString());
            var controller = new ProductController(db, null);

            // Act
            var result = controller.Delete(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void DeleteGet_ShouldReturnViewWithModel_WhenProductExists()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using (var db = GetDbContext(dbName))
            {
                db.Manufacturer.Add(new Manufacturer { Id = 1, Name = "M1", Country = "PL" });
                db.ProductType.Add(new ProductType { Id = 1, Name = "T1" });
                db.Product.Add(new Product { Id = 10, Name = "Wino Do Skasowania", Price = 30, Image = "del.png", IdManufacturer = 1, IdProductType = 1 });
                db.SaveChanges();
            }

            using (var db = GetDbContext(dbName))
            {
                var controller = new ProductController(db, null);

                // Act
                var result = controller.Delete(10);

                // Assert
                var viewResult = Assert.IsType<ViewResult>(result);
                var model = Assert.IsType<ProductVM>(viewResult.Model);
                Assert.Equal(10, model.Product.Id);
            }
        }

        [Fact]
        public void DeleteConfirm_ShouldReturnNotFound_WhenProductDoesNotExist()
        {
            // Arrange
            var db = GetDbContext(Guid.NewGuid().ToString());
            var controller = new ProductController(db, null);
            var productVm = new ProductVM { Product = new Product { Id = 999 } };

            // Act
            var result = controller.DeleteConfirm(productVm);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void DeleteConfirm_ShouldRemoveProductAndRelationsAndFile_WhenInvoked()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using (var db = GetDbContext(dbName))
            {
                db.Product.Add(new Product { Id = 5, Name = "Wino Do Usuniecia", Price = 10, Image = "del.png", IdManufacturer = 1, IdProductType = 1 });
                db.Rating.Add(new Rating { Id = 1, IdProduct = 5, RatingValue = 5, IdCustomer = "test-user-id" });
                db.Comment.Add(new Comment { Id = 1, IdProduct = 5, CommentContent = "Super", IdCustomer = "test-user-id" });
                db.SaveChanges();
            }

            var webHostEnvMock = new Mock<IWebHostEnvironment>();
            string tempWebRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempWebRoot);
            webHostEnvMock.Setup(w => w.WebRootPath).Returns(tempWebRoot);

            string expectedUploadPath = $"{tempWebRoot}{WC.ImageProductPath}";
            Directory.CreateDirectory(expectedUploadPath);

            string imageFilePath = Path.Combine(expectedUploadPath, "del.png");
            File.WriteAllText(imageFilePath, "fake image content");

            using (var db = GetDbContext(dbName))
            {
                var controller = new ProductController(db, webHostEnvMock.Object);
                var productVm = new ProductVM { Product = new Product { Id = 5 } };

                // Act
                var result = controller.DeleteConfirm(productVm);

                // Assert
                var redirectResult = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal("Index", redirectResult.ActionName);

                Assert.Empty(db.Product);
                Assert.Empty(db.Rating);
                Assert.Empty(db.Comment);
                Assert.False(File.Exists(imageFilePath));
            }
        }
    }
}
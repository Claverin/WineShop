using System;
using System.Collections.Generic;
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
    public class ProductDetailsServiceTests
    {
        private ApplicationDbContext GetInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetAsync_ShouldReturnNull_WhenProductDoesNotExist()
        {
            // Arrange
            var dbName = nameof(GetAsync_ShouldReturnNull_WhenProductDoesNotExist);
            using var db = GetInMemoryDbContext(dbName);

            var cartServiceMock = new Mock<ICartService>();
            var ratingServiceMock = new Mock<IRatingService>();

            var service = new ProductDetailsService(db, cartServiceMock.Object, ratingServiceMock.Object);

            // Act
            var result = await service.GetAsync(999, "user-1");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAsync_ShouldReturnViewModelWithoutUserRating_WhenUserIdIsNull()
        {
            // Arrange
            var dbName = nameof(GetAsync_ShouldReturnViewModelWithoutUserRating_WhenUserIdIsNull);
            using var db = GetInMemoryDbContext(dbName);

            db.ProductType.Add(new ProductType { Id = 1, Name = "White Wine" });
            db.Manufacturer.Add(new Manufacturer { Id = 1, Name = "Vineyard X", Country = "France" });

            db.Product.Add(new Product
            {
                Id = 1,
                Name = "Chardonnay",
                Price = 60.0m,
                Image = "chardonnay.jpg",
                IdProductType = 1,
                IdManufacturer = 1,
                Comment = new List<Comment>(),
                Rating = new List<Rating>()
            });
            await db.SaveChangesAsync();

            var cartServiceMock = new Mock<ICartService>();
            cartServiceMock.Setup(c => c.Contains(1)).Returns(true);
            cartServiceMock.Setup(c => c.GetQuantity(1)).Returns(4);

            var ratingServiceMock = new Mock<IRatingService>();

            var service = new ProductDetailsService(db, cartServiceMock.Object, ratingServiceMock.Object);

            // Act
            var result = await service.GetAsync(1, null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Product.Id);
            Assert.Equal("Chardonnay", result.Product.Name);
            Assert.True(result.ExistsInCart);
            Assert.Equal(4, result.CartQuantity);
            Assert.Equal(0, result.UserRating);

            ratingServiceMock.Verify(r => r.GetUserRatingAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetAsync_ShouldReturnViewModelWithUserRating_WhenUserIdIsProvided()
        {
            // Arrange
            var dbName = nameof(GetAsync_ShouldReturnViewModelWithUserRating_WhenUserIdIsProvided);
            using var db = GetInMemoryDbContext(dbName);

            db.ProductType.Add(new ProductType { Id = 1, Name = "Red Wine" });
            db.Manufacturer.Add(new Manufacturer { Id = 1, Name = "Vineyard Y", Country = "Italy" });

            db.Product.Add(new Product
            {
                Id = 2,
                Name = "Merlot",
                Price = 75.0m,
                Image = "merlot.jpg",
                IdProductType = 1,
                IdManufacturer = 1,
                Comment = new List<Comment>(),
                Rating = new List<Rating>()
            });
            await db.SaveChangesAsync();

            var cartServiceMock = new Mock<ICartService>();
            cartServiceMock.Setup(c => c.Contains(2)).Returns(false);
            cartServiceMock.Setup(c => c.GetQuantity(2)).Returns(0);

            var ratingServiceMock = new Mock<IRatingService>();
            ratingServiceMock
                .Setup(r => r.GetUserRatingAsync("user-1", 2))
                .ReturnsAsync(5);

            var service = new ProductDetailsService(db, cartServiceMock.Object, ratingServiceMock.Object);

            // Act
            var result = await service.GetAsync(2, "user-1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Product.Id);
            Assert.Equal("Merlot", result.Product.Name);
            Assert.False(result.ExistsInCart);
            Assert.Equal(0, result.CartQuantity);
            Assert.Equal(5, result.UserRating);

            ratingServiceMock.Verify(r => r.GetUserRatingAsync("user-1", 2), Times.Once);
        }
    }
}
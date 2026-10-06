using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WineShop.Data;
using WineShop.Models;
using WineShop.Services;
using Xunit;

namespace WineShop.Tests.Services
{
    public class RatingServiceTests
    {
        private ApplicationDbContext GetInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetUserRatingAsync_ShouldReturnRatingValue_WhenRatingExists()
        {
            // Arrange
            var dbName = nameof(GetUserRatingAsync_ShouldReturnRatingValue_WhenRatingExists);
            using var db = GetInMemoryDbContext(dbName);

            db.Rating.Add(new Rating
            {
                IdCustomer = "user-1",
                IdProduct = 10,
                RatingValue = 4
            });
            await db.SaveChangesAsync();

            var service = new RatingService(db);

            // Act
            var result = await service.GetUserRatingAsync("user-1", 10);

            // Assert
            Assert.Equal(4, result);
        }

        [Fact]
        public async Task GetUserRatingAsync_ShouldReturnZero_WhenRatingDoesNotExist()
        {
            // Arrange
            var dbName = nameof(GetUserRatingAsync_ShouldReturnZero_WhenRatingDoesNotExist);
            using var db = GetInMemoryDbContext(dbName);

            var service = new RatingService(db);

            // Act
            var result = await service.GetUserRatingAsync("user-1", 10);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task SetRatingAsync_ShouldAddNewRating_WhenRatingDoesNotExistAndRateGreaterThanZero()
        {
            // Arrange
            var dbName = nameof(SetRatingAsync_ShouldAddNewRating_WhenRatingDoesNotExistAndRateGreaterThanZero);
            using var db = GetInMemoryDbContext(dbName);

            var service = new RatingService(db);

            // Act
            await service.SetRatingAsync("user-1", 5, 5);

            // Assert
            var savedRating = await db.Rating.FirstOrDefaultAsync(r => r.IdCustomer == "user-1" && r.IdProduct == 5);
            Assert.NotNull(savedRating);
            Assert.Equal(5, savedRating.RatingValue);
        }

        [Fact]
        public async Task SetRatingAsync_ShouldUpdateExistingRating_WhenRatingExistsAndRateGreaterThanZero()
        {
            // Arrange
            var dbName = nameof(SetRatingAsync_ShouldUpdateExistingRating_WhenRatingExistsAndRateGreaterThanZero);
            using var db = GetInMemoryDbContext(dbName);

            db.Rating.Add(new Rating
            {
                IdCustomer = "user-1",
                IdProduct = 5,
                RatingValue = 2
            });
            await db.SaveChangesAsync();

            var service = new RatingService(db);

            // Act
            await service.SetRatingAsync("user-1", 5, 4);

            // Assert
            var updatedRating = await db.Rating.FirstOrDefaultAsync(r => r.IdCustomer == "user-1" && r.IdProduct == 5);
            Assert.NotNull(updatedRating);
            Assert.Equal(4, updatedRating.RatingValue);
        }

        [Fact]
        public async Task SetRatingAsync_ShouldRemoveExistingRating_WhenRateIsZeroAndRatingExists()
        {
            // Arrange
            var dbName = nameof(SetRatingAsync_ShouldRemoveExistingRating_WhenRateIsZeroAndRatingExists);
            using var db = GetInMemoryDbContext(dbName);

            db.Rating.Add(new Rating
            {
                IdCustomer = "user-1",
                IdProduct = 5,
                RatingValue = 3
            });
            await db.SaveChangesAsync();

            var service = new RatingService(db);

            // Act
            await service.SetRatingAsync("user-1", 5, 0);

            // Assert
            var removedRating = await db.Rating.FirstOrDefaultAsync(r => r.IdCustomer == "user-1" && r.IdProduct == 5);
            Assert.Null(removedRating);
        }

        [Fact]
        public async Task SetRatingAsync_ShouldDoNothing_WhenRateIsZeroAndRatingDoesNotExist()
        {
            // Arrange
            var dbName = nameof(SetRatingAsync_ShouldDoNothing_WhenRateIsZeroAndRatingDoesNotExist);
            using var db = GetInMemoryDbContext(dbName);

            var service = new RatingService(db);

            // Act
            await service.SetRatingAsync("user-1", 5, 0);

            // Assert
            var count = await db.Rating.CountAsync();
            Assert.Equal(0, count);
        }
    }
}
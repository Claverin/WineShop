using Microsoft.EntityFrameworkCore;
using Xunit;
using WineShop.Data;
using WineShop.Models;
using WineShop.Services;
using WineShop.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace WineShop.Tests.Services
{
    public class CommentServiceTests
    {
        private ApplicationDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task AddAsync_ShouldAddCommentAndReturnProductId()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var db = GetDbContext(dbName);
            var service = new CommentService(db);

            string userId = "user-123";
            int productId = 5;
            string content = "Great wine!";

            // Act
            var resultProductId = await service.AddAsync(userId, productId, content);

            // Assert
            Assert.Equal(productId, resultProductId);
            Assert.Single(db.Comment);
            var commentInDb = await db.Comment.FirstAsync();
            Assert.Equal(content, commentInDb.CommentContent);
            Assert.Equal(userId, commentInDb.IdCustomer);
            Assert.Equal(productId, commentInDb.IdProduct);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnNotFound_WhenCommentDoesNotExist()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var db = GetDbContext(dbName);
            var service = new CommentService(db);

            // Act
            var result = await service.DeleteAsync(999, "user-123", false);

            // Assert
            Assert.Equal(DeleteCommentStatus.NotFound, result.Status);
            Assert.Null(result.ProductId);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnForbidden_WhenUserIsNotOwnerAndNotAdmin()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using (var db = GetDbContext(dbName))
            {
                db.Comment.Add(new Comment
                {
                    Id = 1,
                    IdCustomer = "owner-id",
                    IdProduct = 10,
                    CommentContent = "Test"
                });
                db.SaveChanges();
            }

            using var context = GetDbContext(dbName);
            var service = new CommentService(context);

            // Act
            var result = await service.DeleteAsync(1, "other-user-id", false);

            // Assert
            Assert.Equal(DeleteCommentStatus.Forbidden, result.Status);
            Assert.Equal(10, result.ProductId);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteComment_WhenUserIsOwner()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using (var db = GetDbContext(dbName))
            {
                db.Comment.Add(new Comment
                {
                    Id = 1,
                    IdCustomer = "owner-id",
                    IdProduct = 10,
                    CommentContent = "Test"
                });
                db.SaveChanges();
            }

            using (var db = GetDbContext(dbName))
            {
                var service = new CommentService(db);

                // Act
                var result = await service.DeleteAsync(1, "owner-id", false);

                // Assert
                Assert.Equal(DeleteCommentStatus.Success, result.Status);
                Assert.Equal(10, result.ProductId);
                Assert.Empty(db.Comment);
            }
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteComment_WhenUserIsAdminEvenIfNotOwner()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using (var db = GetDbContext(dbName))
            {
                db.Comment.Add(new Comment
                {
                    Id = 1,
                    IdCustomer = "owner-id",
                    IdProduct = 10,
                    CommentContent = "Test"
                });
                db.SaveChanges();
            }

            using (var db = GetDbContext(dbName))
            {
                var service = new CommentService(db);

                // Act
                var result = await service.DeleteAsync(1, "admin-user-id", true);

                // Assert
                Assert.Equal(DeleteCommentStatus.Success, result.Status);
                Assert.Equal(10, result.ProductId);
                Assert.Empty(db.Comment);
            }
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using WineShop.Controllers;
using WineShop.Data;
using WineShop.Models;
using WineShop.Models.ViewModels;
using WineShop.Services.Interfaces;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Diagnostics;
using System;
using System.Linq;

namespace WineShop.Tests.Controllers
{
    public class HomeControllerTests
    {
        private ApplicationDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public void Index_ShouldReturnViewResult()
        {
            // Arrange
            var controller = new HomeController(null, null, null, null, null, null);

            // Act
            var result = controller.Index();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void ShopSite_ShouldReturnViewWithPagedProducts_WhenNoFilter()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using (var db = GetDbContext(dbName))
            {
                db.Manufacturer.Add(new Manufacturer { Id = 1, Name = "M1", Country = "PL" });
                db.ProductType.Add(new ProductType { Id = 1, Name = "Czerwone" });
                db.Product.Add(new Product { Id = 1, Name = "Wino A", Price = 30, Image = "wino1.png", IdManufacturer = 1, IdProductType = 1 });
                db.Product.Add(new Product { Id = 2, Name = "Wino B", Price = 40, Image = "wino2.png", IdManufacturer = 1, IdProductType = 1 });
                db.SaveChanges();
            }

            using (var db = GetDbContext(dbName))
            {
                var controller = new HomeController(null, db, null, null, null, null);

                // Act
                var result = controller.ShopSite(typeId: null, page: 1, pageSize: 10);

                // Assert
                var viewResult = Assert.IsType<ViewResult>(result);
                var model = Assert.IsType<HomeVM>(viewResult.Model);
                Assert.Equal(2, model.TotalItems);
                Assert.Equal(2, model.Products.Count());
            }
        }

        [Fact]
        public void ShopSite_ShouldFilterByType_WhenTypeIdIsProvided()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using (var db = GetDbContext(dbName))
            {
                db.Manufacturer.Add(new Manufacturer { Id = 1, Name = "M1", Country = "PL" });
                db.ProductType.Add(new ProductType { Id = 1, Name = "Czerwone" });
                db.ProductType.Add(new ProductType { Id = 2, Name = "Białe" });
                db.Product.Add(new Product { Id = 1, Name = "Wino Czerwone", Price = 30, Image = "czerwone.png", IdManufacturer = 1, IdProductType = 1 });
                db.Product.Add(new Product { Id = 2, Name = "Wino Białe", Price = 40, Image = "biale.png", IdManufacturer = 1, IdProductType = 2 });
                db.SaveChanges();
            }

            using (var db = GetDbContext(dbName))
            {
                var controller = new HomeController(null, db, null, null, null, null);

                // Act
                var result = controller.ShopSite(typeId: 2, page: 1, pageSize: 10);

                // Assert
                var viewResult = Assert.IsType<ViewResult>(result);
                var model = Assert.IsType<HomeVM>(viewResult.Model);
                Assert.Equal(1, model.TotalItems);
                Assert.Equal("Wino Białe", model.Products.First().Name);
            }
        }

        [Fact]
        public async Task Details_ShouldReturnNotFound_WhenProductIsNull()
        {
            // Arrange
            var productDetailsServiceMock = new Mock<IProductDetailsService>();
            productDetailsServiceMock.Setup(s => s.GetAsync(It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync((DetailsVM)null);

            var controller = new HomeController(null, null, null, null, null, productDetailsServiceMock.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

            // Act
            var result = await controller.Details(99);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_ShouldReturnViewWithModel_WhenProductExists()
        {
            // Arrange
            var expectedVm = new DetailsVM();
            string userId = "user-123";
            var productDetailsServiceMock = new Mock<IProductDetailsService>();
            productDetailsServiceMock.Setup(s => s.GetAsync(1, userId)).ReturnsAsync(expectedVm);

            var controller = new HomeController(null, null, null, null, null, productDetailsServiceMock.Object);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }));
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

            // Act
            var result = await controller.Details(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(expectedVm, viewResult.Model);
        }

        [Fact]
        public void DetailsPost_ShouldAddCartAndRedirect_WhenInvoked()
        {
            // Arrange
            var cartServiceMock = new Mock<ICartService>();
            var controller = new HomeController(null, null, cartServiceMock.Object, null, null, null);

            // Act
            var result = controller.DetailsPost(5, 3);

            // Assert
            cartServiceMock.Verify(s => s.Add(5, 3), Times.Once);
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirectToActionResult.ActionName);
            Assert.Equal(5, redirectToActionResult.RouteValues["id"]);
        }

        [Fact]
        public void RemoveFromCart_ShouldCallServiceAndRedirect_WhenInvoked()
        {
            // Arrange
            var cartServiceMock = new Mock<ICartService>();
            var controller = new HomeController(null, null, cartServiceMock.Object, null, null, null);

            // Act
            var result = controller.RemoveFromCart(5);

            // Assert
            cartServiceMock.Verify(s => s.Remove(5), Times.Once);
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ShopSite", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task AddComment_ShouldReturnForbid_WhenUserIsNotAuthenticated()
        {
            // Arrange
            var controller = new HomeController(null, null, null, null, null, null);
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

            // Act
            var result = await controller.AddComment(new Comment());

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task AddComment_ShouldReturnBadRequest_WhenProductIdIsNull()
        {
            // Arrange
            var controller = new HomeController(null, null, null, null, null, null);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user-1") }));
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

            // Act
            var result = await controller.AddComment(new Comment { IdProduct = null });

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task AddComment_ShouldRedirectToDetails_WhenModelStateIsInvalid()
        {
            // Arrange
            var controller = new HomeController(null, null, null, null, null, null);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user-1") }));
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
            controller.ModelState.AddModelError("CommentContent", "Required");

            // Act
            var result = await controller.AddComment(new Comment { IdProduct = 10 });

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirectResult.ActionName);
            Assert.Equal(10, redirectResult.RouteValues["id"]);
        }

        [Fact]
        public async Task AddComment_ShouldAddCommentAndRedirect_WhenValid()
        {
            // Arrange
            string userId = "user-1";
            var commentServiceMock = new Mock<ICommentService>();
            commentServiceMock.Setup(s => s.AddAsync(userId, 10, "Super wino")).ReturnsAsync(10);

            var controller = new HomeController(null, null, null, null, commentServiceMock.Object, null);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }));
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

            var comment = new Comment { IdProduct = 10, CommentContent = "Super wino" };

            // Act
            var result = await controller.AddComment(comment);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirectResult.ActionName);
            Assert.Equal(10, redirectResult.RouteValues["id"]);
        }

        [Fact]
        public async Task AddComment_ShouldHandleNullCommentContent_WhenValid()
        {
            // Arrange
            string userId = "user-1";
            var commentServiceMock = new Mock<ICommentService>();
            commentServiceMock.Setup(s => s.AddAsync(userId, 10, string.Empty)).ReturnsAsync(10);

            var controller = new HomeController(null, null, null, null, commentServiceMock.Object, null);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }));
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

            var comment = new Comment { IdProduct = 10, CommentContent = null };

            // Act
            var result = await controller.AddComment(comment);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirectResult.ActionName);
            Assert.Equal(10, redirectResult.RouteValues["id"]);
        }

        [Fact]
        public async Task DeleteComment_ShouldReturnNotFound_WhenStatusIsNotFound()
        {
            // Arrange
            var commentServiceMock = new Mock<ICommentService>();
            commentServiceMock.Setup(s => s.DeleteAsync(1, "user-1", false))
                .ReturnsAsync(new DeleteCommentResult(DeleteCommentStatus.NotFound, null));

            var controller = new HomeController(null, null, null, null, commentServiceMock.Object, null);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user-1") }));
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

            // Act
            var result = await controller.DeleteComment(1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteComment_ShouldReturnForbid_WhenStatusIsForbidden()
        {
            // Arrange
            var commentServiceMock = new Mock<ICommentService>();
            commentServiceMock.Setup(s => s.DeleteAsync(1, "user-1", false))
                .ReturnsAsync(new DeleteCommentResult(DeleteCommentStatus.Forbidden, 5));

            var controller = new HomeController(null, null, null, null, commentServiceMock.Object, null);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user-1") }));
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

            // Act
            var result = await controller.DeleteComment(1);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task DeleteComment_ShouldRedirectToShopSite_WhenProductIdIsNull()
        {
            // Arrange
            var commentServiceMock = new Mock<ICommentService>();
            commentServiceMock.Setup(s => s.DeleteAsync(1, "user-1", false))
                .ReturnsAsync(new DeleteCommentResult(DeleteCommentStatus.Success, null));

            var controller = new HomeController(null, null, null, null, commentServiceMock.Object, null);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user-1") }));
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

            // Act
            var result = await controller.DeleteComment(1);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ShopSite", redirectResult.ActionName);
        }

        [Fact]
        public async Task DeleteComment_ShouldRedirectToDetails_WhenSuccessWithProductId()
        {
            // Arrange
            var commentServiceMock = new Mock<ICommentService>();
            commentServiceMock.Setup(s => s.DeleteAsync(1, "user-1", false))
                .ReturnsAsync(new DeleteCommentResult(DeleteCommentStatus.Success, 7));

            var controller = new HomeController(null, null, null, null, commentServiceMock.Object, null);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user-1") }));
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

            // Act
            var result = await controller.DeleteComment(1);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirectResult.ActionName);
            Assert.Equal(7, redirectResult.RouteValues["id"]);
        }

        [Fact]
        public async Task DeleteComment_ShouldUseEmptyString_WhenNameIdentifierIsNull()
        {
            // Arrange
            var commentServiceMock = new Mock<ICommentService>();
            commentServiceMock.Setup(s => s.DeleteAsync(1, string.Empty, false))
                .ReturnsAsync(new DeleteCommentResult(DeleteCommentStatus.NotFound, null));

            var controller = new HomeController(null, null, null, null, commentServiceMock.Object, null);

            // Brak Claimu z ID uzytkownika
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) } };

            // Act
            var result = await controller.DeleteComment(1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task RateProduct_ShouldReturnForbid_WhenUserIsNotAuthenticated()
        {
            // Arrange
            var controller = new HomeController(null, null, null, null, null, null);
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

            // Act
            var result = await controller.RateProduct(1, 5);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task RateProduct_ShouldReturnBadRequest_WhenRateIsOutOfRange()
        {
            // Arrange
            var controller = new HomeController(null, null, null, null, null, null);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user-1") }));
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

            // Act
            var result1 = await controller.RateProduct(1, -1);
            var result2 = await controller.RateProduct(1, 6);

            // Assert
            Assert.IsType<BadRequestResult>(result1);
            Assert.IsType<BadRequestResult>(result2);
        }

        [Fact]
        public async Task RateProduct_ShouldReturnOk_WhenValid()
        {
            // Arrange
            string userId = "user-1";
            var ratingServiceMock = new Mock<IRatingService>();
            var controller = new HomeController(null, null, null, ratingServiceMock.Object, null, null);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }));
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

            // Act
            var result = await controller.RateProduct(1, 4);

            // Assert
            ratingServiceMock.Verify(s => s.SetRatingAsync(userId, 1, 4), Times.Once);
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public void Error_ShouldReturnViewWithErrorViewModel_WhenActivityCurrentIsNull()
        {
            // Arrange
            var controller = new HomeController(null, null, null, null, null, null);
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

            Activity.Current = null;

            // Act
            var result = controller.Error();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ErrorViewModel>(viewResult.Model);
            Assert.NotNull(model.RequestId);
        }

        [Fact]
        public void Error_ShouldReturnViewWithErrorViewModel_WhenActivityCurrentIsNotNull()
        {
            // Arrange
            var controller = new HomeController(null, null, null, null, null, null);
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

            using var activity = new Activity("TestActivity");
            activity.Start();

            // Act
            var result = controller.Error();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ErrorViewModel>(viewResult.Model);
            Assert.Equal(activity.Id, model.RequestId);

            activity.Stop();
        }
    }
}
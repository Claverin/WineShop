using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using WineShop.Controllers;
using WineShop.Data;
using WineShop.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using WineShop.Models;
using WineShop.Models.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace WineShop.Tests.Controllers
{
    public class CartControllerTests
    {
        [Fact]
        public void Remove_ShouldCallCartServiceAndRedirectToIndex_WhenInvoked()
        {
            // Arrange
            var mockCartService = new Mock<ICartService>();
            var mockOrderService = new Mock<IOrderService>();
            var controller = new CartController(null, mockCartService.Object, mockOrderService.Object);
            int productIdToRemove = 5;

            // Act
            var result = controller.Remove(productIdToRemove);

            // Assert
            mockCartService.Verify(service => service.Remove(productIdToRemove), Times.Once);
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }

        [Fact]
        public void Increase_ShouldCallCartServiceAndRedirectToIndex_WhenInvoked()
        {
            // Arrange
            var mockCartService = new Mock<ICartService>();
            var mockOrderService = new Mock<IOrderService>();
            var controller = new CartController(null, mockCartService.Object, mockOrderService.Object);
            int productId = 1;

            // Act
            var result = controller.Increase(productId);

            // Assert
            mockCartService.Verify(service => service.Increase(productId), Times.Once);
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }

        [Fact]
        public void Decrease_ShouldCallCartServiceAndRedirectToIndex_WhenInvoked()
        {
            // Arrange
            var mockCartService = new Mock<ICartService>();
            var mockOrderService = new Mock<IOrderService>();
            var controller = new CartController(null, mockCartService.Object, mockOrderService.Object);
            int productId = 1;

            // Act
            var result = controller.Decrease(productId);

            // Assert
            mockCartService.Verify(service => service.Decrease(productId), Times.Once);
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }

        [Fact]
        public void IndexPost_ShouldRedirectToSummary_WhenInvoked()
        {
            // Arrange
            var controller = new CartController(null, null, null);

            // Act
            var result = controller.IndexPost();

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Summary", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task Index_ShouldReturnViewResultWithCartVM_WhenCartHasItems()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                var testManufacturer = new Manufacturer { Id = 1, Name = "Winnica Testowa", Country = "Polska" };
                var testProductType = new ProductType { Id = 1, Name = "Wino Czerwone" };

                context.Manufacturer.Add(testManufacturer);
                context.ProductType.Add(testProductType);

                context.Product.Add(new Product
                {
                    Id = 1,
                    Name = "Wino Czerwone",
                    Price = 50.0m,
                    Image = "czerwone.png",
                    IdManufacturer = 1,
                    IdProductType = 1
                });

                context.Product.Add(new Product
                {
                    Id = 2,
                    Name = "Wino Białe",
                    Price = 40.0m,
                    Image = "biale.png",
                    IdManufacturer = 1,
                    IdProductType = 1
                });

                context.SaveChanges();
            }

            var mockCartService = new Mock<ICartService>();
            var fakeCart = new List<ShoppingCart>
            {
                new ShoppingCart { ProductId = 1, Quantity = 2 }
            };
            mockCartService.Setup(s => s.GetAll()).Returns(fakeCart);

            var mockOrderService = new Mock<IOrderService>();

            using (var context = new ApplicationDbContext(options))
            {
                var controller = new CartController(context, mockCartService.Object, mockOrderService.Object);

                // Act
                var result = await controller.Index();

                // Assert
                var viewResult = Assert.IsType<ViewResult>(result);
                var model = Assert.IsAssignableFrom<CartVM>(viewResult.Model);

                Assert.Single(model.Items);
                Assert.Equal("Wino Czerwone", model.Items[0].Name);
                Assert.Equal(2, model.Items[0].Quantity);
            }
        }

        [Fact]
        public async Task Summary_ShouldReturnForbid_WhenUserIsNotAuthenticated()
        {
            // Arrange
            var mockOrderService = new Mock<IOrderService>();
            var controller = new CartController(null, null, mockOrderService.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

            // Act
            var result = await controller.Summary();

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task Summary_ShouldRedirectToIndex_WhenCartIsEmpty()
        {
            // Arrange
            var mockOrderService = new Mock<IOrderService>();
            var checkoutVm = new CheckoutVM { Items = new List<CheckoutItemVM>() };

            mockOrderService.Setup(s => s.BuildCheckoutAsync(It.IsAny<string>())).ReturnsAsync(checkoutVm);

            var controller = new CartController(null, null, mockOrderService.Object);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user-123") }));
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

            // Act
            var result = await controller.Summary();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public async Task Summary_ShouldReturnViewResultWithModel_WhenCartIsNotEmpty()
        {
            // Arrange
            var mockOrderService = new Mock<IOrderService>();
            var checkoutVm = new CheckoutVM { Items = new List<CheckoutItemVM> { new CheckoutItemVM() } };

            mockOrderService.Setup(s => s.BuildCheckoutAsync(It.IsAny<string>())).ReturnsAsync(checkoutVm);

            var controller = new CartController(null, null, mockOrderService.Object);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user-id-123") }));
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

            // Act
            var result = await controller.Summary();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(checkoutVm, viewResult.Model);
        }

        [Fact]
        public async Task SummaryPost_ShouldReturnForbid_WhenUserIsNotAuthenticated()
        {
            // Arrange
            var controller = new CartController(null, null, null);
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

            // Act
            var result = await controller.SummaryPost(new CheckoutVM());

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task SummaryPost_ShouldRedirectToIndex_WhenCartIsEmpty()
        {
            // Arrange
            var mockOrderService = new Mock<IOrderService>();
            var checkoutVm = new CheckoutVM { Items = new List<CheckoutItemVM>() };

            mockOrderService.Setup(s => s.BuildCheckoutAsync(It.IsAny<string>())).ReturnsAsync(checkoutVm);

            var controller = new CartController(null, null, mockOrderService.Object);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user-123") }));
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

            // Act
            var result = await controller.SummaryPost(new CheckoutVM());

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public async Task SummaryPost_ShouldReturnViewWithModel_WhenModelStateIsInvalid()
        {
            // Arrange
            var mockOrderService = new Mock<IOrderService>();
            var checkoutVm = new CheckoutVM { Items = new List<CheckoutItemVM> { new CheckoutItemVM() } };

            mockOrderService.Setup(s => s.BuildCheckoutAsync(It.IsAny<string>())).ReturnsAsync(checkoutVm);

            var controller = new CartController(null, null, mockOrderService.Object);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user-123") }));
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

            controller.ModelState.AddModelError("Address", "Address is required");

            var inputModel = new CheckoutVM();

            // Act
            var result = await controller.SummaryPost(inputModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var returnedModel = Assert.IsType<CheckoutVM>(viewResult.Model);
            Assert.Equal(checkoutVm.Items, returnedModel.Items);
        }

        [Fact]
        public async Task SummaryPost_ShouldReturnViewWithError_WhenOrderCreationFails()
        {
            // Arrange
            var mockOrderService = new Mock<IOrderService>();
            var checkoutVm = new CheckoutVM { Items = new List<CheckoutItemVM> { new CheckoutItemVM() } };

            mockOrderService.Setup(s => s.BuildCheckoutAsync(It.IsAny<string>())).ReturnsAsync(checkoutVm);
            mockOrderService.Setup(s => s.PlaceOrderAsync(It.IsAny<CheckoutVM>(), It.IsAny<string>())).ReturnsAsync((int?)null);

            var controller = new CartController(null, null, mockOrderService.Object);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user-123") }));
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

            // Act
            var result = await controller.SummaryPost(new CheckoutVM());

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
            Assert.True(controller.ModelState.ContainsKey(string.Empty));
        }

        [Fact]
        public async Task SummaryPost_ShouldRedirectToConfirmation_WhenOrderIsPlacedSuccessfully()
        {
            // Arrange
            var mockOrderService = new Mock<IOrderService>();
            var checkoutVm = new CheckoutVM { Items = new List<CheckoutItemVM> { new CheckoutItemVM() } };

            mockOrderService.Setup(s => s.BuildCheckoutAsync(It.IsAny<string>())).ReturnsAsync(checkoutVm);
            mockOrderService.Setup(s => s.PlaceOrderAsync(It.IsAny<CheckoutVM>(), It.IsAny<string>())).ReturnsAsync(99);

            var controller = new CartController(null, null, mockOrderService.Object);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user-id-123") }));
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

            // Act
            var result = await controller.SummaryPost(checkoutVm);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Confirmation", redirectResult.ActionName);
            Assert.Equal(99, redirectResult.RouteValues["id"]);
        }

        [Fact]
        public async Task Confirmation_ShouldReturnForbid_WhenUserIsNotAuthenticated()
        {
            // Arrange
            var controller = new CartController(null, null, null);
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

            // Act
            var result = await controller.Confirmation(1);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task Confirmation_ShouldReturnNotFound_WhenOrderDoesNotExist()
        {
            // Arrange
            var mockOrderService = new Mock<IOrderService>();
            mockOrderService.Setup(s => s.GetConfirmationAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()))
                            .ReturnsAsync((OrderConfirmationVM)null);

            var controller = new CartController(null, null, mockOrderService.Object);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "user-id-123"),
                new Claim(ClaimTypes.Role, "Customer")
            }));
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

            // Act
            var result = await controller.Confirmation(99);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Confirmation_ShouldReturnViewWithModel_WhenOrderExists()
        {
            // Arrange
            var mockOrderService = new Mock<IOrderService>();
            var expectedConfirmationVm = new OrderConfirmationVM();

            mockOrderService.Setup(s => s.GetConfirmationAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()))
                            .ReturnsAsync(expectedConfirmationVm);

            var controller = new CartController(null, null, mockOrderService.Object);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "user-123"),
                new Claim(ClaimTypes.Role, "Customer")
            }));
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

            // Act
            var result = await controller.Confirmation(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using WineShop.Controllers;
using WineShop.Models.ViewModels;
using WineShop.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WineShop.Tests.Controllers
{
    public class OrdersControllerTests
    {
        [Fact]
        public async Task Index_ShouldReturnViewWithOrders()
        {
            // Arrange
            var mockAdminOrderService = new Mock<IAdminOrderService>();
            var expectedOrders = new List<AdminOrderListItemVM>
            {
                new AdminOrderListItemVM { OrderId = 1, CustomerName = "John Doe" },
                new AdminOrderListItemVM { OrderId = 2, CustomerName = "Jane Doe" }
            };

            mockAdminOrderService.Setup(s => s.GetOrdersAsync()).ReturnsAsync(expectedOrders);

            var controller = new OrdersController(mockAdminOrderService.Object);

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<AdminOrderListItemVM>>(viewResult.Model);
            Assert.Equal(2, model.Count());
        }

        [Fact]
        public async Task Details_ShouldReturnNotFound_WhenOrderDoesNotExist()
        {
            // Arrange
            var mockAdminOrderService = new Mock<IAdminOrderService>();
            mockAdminOrderService.Setup(s => s.GetOrderAsync(It.IsAny<int>()))
                                 .ReturnsAsync((AdminOrderDetailsVM)null);

            var controller = new OrdersController(mockAdminOrderService.Object);

            // Act
            var result = await controller.Details(99);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_ShouldReturnViewWithModel_WhenOrderExists()
        {
            // Arrange
            var mockAdminOrderService = new Mock<IAdminOrderService>();
            var expectedOrder = new AdminOrderDetailsVM { OrderId = 1, CustomerName = "John Doe" };

            mockAdminOrderService.Setup(s => s.GetOrderAsync(1))
                                 .ReturnsAsync(expectedOrder);

            var controller = new OrdersController(mockAdminOrderService.Object);

            // Act
            var result = await controller.Details(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<AdminOrderDetailsVM>(viewResult.Model);
            Assert.Equal(1, model.OrderId);
        }

        [Fact]
        public async Task Update_ShouldReturnNotFound_WhenUpdateFails()
        {
            // Arrange
            var mockAdminOrderService = new Mock<IAdminOrderService>();
            mockAdminOrderService.Setup(s => s.UpdateOrderAsync(It.IsAny<AdminOrderDetailsVM>()))
                                 .ReturnsAsync(false);

            var controller = new OrdersController(mockAdminOrderService.Object);
            var modelToUpdate = new AdminOrderDetailsVM { OrderId = 99 };

            // Act
            var result = await controller.Update(modelToUpdate);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Update_ShouldRedirectToDetails_WhenUpdateSucceeds()
        {
            // Arrange
            var mockAdminOrderService = new Mock<IAdminOrderService>();
            mockAdminOrderService.Setup(s => s.UpdateOrderAsync(It.IsAny<AdminOrderDetailsVM>()))
                                 .ReturnsAsync(true);

            var controller = new OrdersController(mockAdminOrderService.Object);
            var modelToUpdate = new AdminOrderDetailsVM { OrderId = 1 };

            // Act
            var result = await controller.Update(modelToUpdate);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirectResult.ActionName);
            Assert.Equal(1, redirectResult.RouteValues["id"]);
        }
    }
}
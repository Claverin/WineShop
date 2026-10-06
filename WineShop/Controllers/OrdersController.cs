using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WineShop.Models.ViewModels;
using WineShop.Services.Interfaces;
using WineShop.Utility;

namespace WineShop.Controllers
{
    [Authorize(Roles = WC.AdminRole)]
    public class OrdersController : Controller
    {
        private readonly IAdminOrderService _adminOrderService;

        public OrdersController(IAdminOrderService adminOrderService)
        {
            _adminOrderService = adminOrderService;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _adminOrderService.GetOrdersAsync();
            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _adminOrderService.GetOrderAsync(id);

            if (order is null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(AdminOrderDetailsVM model)
        {
            var updated = await _adminOrderService.UpdateOrderAsync(model);

            if (!updated)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Details), new { id = model.OrderId });
        }
    }
}
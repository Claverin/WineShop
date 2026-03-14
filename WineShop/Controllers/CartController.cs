using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WineShop.Data;
using WineShop.Models.ViewModels;
using WineShop.Services.Interfaces;
using WineShop.Utility;

namespace WineShop.Controllers
{
    [Authorize(Roles = WC.AdminRole + "," + WC.CustomerRole)]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;

        public CartController(
            ApplicationDbContext db,
            ICartService cartService,
            IOrderService orderService)
        {
            _db = db;
            _cartService = cartService;
            _orderService = orderService;
        }

        public async Task<IActionResult> Index()
        {
            var productIds = _cartService.GetProductIds();

            var products = await _db.Product
                .AsNoTracking()
                .Where(x => productIds.Contains(x.Id))
                .OrderBy(x => x.Name)
                .ToListAsync();

            return View(products);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Index")]
        public IActionResult IndexPost()
        {
            return RedirectToAction(nameof(Summary));
        }

        public async Task<IActionResult> Summary()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Forbid();
            }

            var vm = await _orderService.BuildCheckoutAsync(userId);

            if (!vm.Items.Any())
            {
                return RedirectToAction(nameof(Index));
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Summary")]
        public async Task<IActionResult> SummaryPost(CheckoutVM model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Forbid();
            }

            var checkoutData = await _orderService.BuildCheckoutAsync(userId);

            if (!checkoutData.Items.Any())
            {
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                model.Items = checkoutData.Items;
                model.PaymentMethods = checkoutData.PaymentMethods;
                return View(model);
            }

            var orderId = await _orderService.PlaceOrderAsync(model, userId);

            if (orderId is null)
            {
                model.Items = checkoutData.Items;
                model.PaymentMethods = checkoutData.PaymentMethods;
                ModelState.AddModelError(string.Empty, "Unable to create order.");
                return View(model);
            }

            return RedirectToAction(nameof(Confirmation), new { id = orderId.Value });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int id)
        {
            _cartService.Remove(id);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Confirmation(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Forbid();
            }

            var vm = await _orderService.GetConfirmationAsync(id, userId, User.IsInRole(WC.AdminRole));

            if (vm is null)
            {
                return NotFound();
            }

            return View(vm);
        }
    }
}
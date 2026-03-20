using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WineShop.Data;
using WineShop.Models.ViewModels;
using WineShop.Utility;

namespace WineShop.Controllers
{
    [Authorize]
    public class ShipmentController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ShipmentController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole(WC.AdminRole);

            var query = _db.Order
                .AsNoTracking()
                .Include(x => x.OrderStatus)
                .Include(x => x.PaymentMethod)
                .AsQueryable();

            if (!isAdmin)
            {
                query = query.Where(x => x.CustomerId == userId);
            }

            var items = await query
                .OrderByDescending(x => x.CreatedAtUtc)
                .Select(x => new ShipmentListItemVM
                {
                    OrderId = x.Id,
                    CustomerEmail = x.CustomerEmail,
                    StatusName = x.OrderStatus.Name,
                    PaymentMethodName = x.PaymentMethod.Name,
                    TotalAmount = x.TotalAmount,
                    CreatedAtUtc = x.CreatedAtUtc,
                    Carrier = x.Carrier,
                    ShippingMethod = x.ShippingMethod,
                    TrackingNumber = x.TrackingNumber,
                    ShippedDate = x.ShippedDate,
                    DeliveredDate = x.DeliveredDate
                })
                .ToListAsync();

            return View(items);
        }
    }
}
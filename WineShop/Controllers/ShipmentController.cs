using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WineShop.Data;
using WineShop.Models;
using WineShop.Models.ViewModels;

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
                .Include(x => x.Shipment)
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
                    SendDate = x.ShipmentId != null ? x.Shipment.SendDate : null,
                    DeliverDate = x.ShipmentId != null ? x.Shipment.DeliverDate : null
                })
                .ToListAsync();

            return View(items);
        }

        [Authorize(Roles = WC.AdminRole)]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = WC.AdminRole)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Shipment obj)
        {
            obj.SendDate = DateTime.Now;

            if (ModelState.IsValid)
            {
                _db.Shipment.Add(obj);
                _db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            return View(obj);
        }

        [Authorize(Roles = WC.AdminRole)]
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var obj = _db.Shipment.Find(id);

            if (obj == null)
            {
                return NotFound();
            }

            return View(obj);
        }

        [Authorize(Roles = WC.AdminRole)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Shipment obj)
        {
            if (obj.DeliverDate < DateTime.Now)
            {
                ModelState.AddModelError("DeliverDate", "Deliver data can't be set lower than send data");
            }

            if (ModelState.IsValid)
            {
                _db.Shipment.Update(obj);
                _db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            return View(obj);
        }

        [Authorize(Roles = WC.AdminRole)]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var obj = _db.Shipment.Find(id);

            if (obj == null)
            {
                return NotFound();
            }

            return View(obj);
        }

        [Authorize(Roles = WC.AdminRole)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            var obj = _db.Shipment.Find(id);

            if (obj == null)
            {
                return NotFound();
            }

            _db.Shipment.Remove(obj);
            _db.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WineShop.Data;
using WineShop.Models;

namespace WineShop.Controllers
{
    [Authorize(Roles = WC.AdminRole)]
    public class PaymentMethodController : Controller
    {
        private readonly ApplicationDbContext _db;

        public PaymentMethodController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            IEnumerable<PaymentMethod> objList = _db.PaymentMethod;
            return View(objList);
        }

        //CREATE
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(PaymentMethod obj)
        {
            if (ModelState.IsValid)
            {
                _db.PaymentMethod.Add(obj);
                _db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(obj);
        }

        //EDIT
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var obj = _db.PaymentMethod.Find(id);
            if (obj == null)
            {
                return NotFound();
            }

            return View(obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(PaymentMethod obj)
        {
            if (ModelState.IsValid)
            {
                _db.PaymentMethod.Update(obj);
                _db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(obj);
        }

        //DELETE
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var obj = _db.PaymentMethod.Find(id);

            if (obj == null)
            {
                return NotFound();
            }
            return View(obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            var obj = _db.PaymentMethod.Find(id);
            if (obj == null)
            {
                return NotFound();
            }
            _db.PaymentMethod.Remove(obj);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using WineShop.Data;
using WineShop.Models;

namespace WineShop.Controllers
{
    public class ManufacturerController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ManufacturerController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            IEnumerable<Manufacturer> objList = _db.Manufacturer;
            return View(objList);
        }

        //GET - CREATE
        public IActionResult Create()
        {
            return View();
        }

        //POST - CREATE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Manufacturer obj)
        {
            if (ModelState.IsValid)
            {
                _db.Manufacturer.Add(obj);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(obj);

        }

        //GET - EDIT
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var obj = _db.Manufacturer.Find(id);
            if (obj == null)
            {
                return NotFound();
            }

            return View(obj);
        }

        //POST - EDIT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Manufacturer obj)
        {
            if (ModelState.IsValid)
            {
                _db.Manufacturer.Update(obj);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(obj);

        }

        //GET - DELETE
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var obj = _db.Manufacturer.Find(id);
            if (obj == null)
            {
                return NotFound();
            }

            return View(obj);
        }

        //POST - DELETE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            var obj = _db.Manufacturer.Find(id);
            if (obj == null)
            {
                return NotFound();
            }
            _db.Manufacturer.Remove(obj);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}

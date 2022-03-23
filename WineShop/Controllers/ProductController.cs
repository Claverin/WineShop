using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WineShop.Data;
using WineShop.Models;
using WineShop.Models.ViewModels;

namespace WineShop.Controllers
{
    [Authorize(Roles = WC.AdminRole)]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            IEnumerable<Product> productList = _db.Product
                .Include(x => x.Manufacturer)
                .Include(x => x.ProductType);
            return View(productList);
        }

        public IActionResult Create()
        {
            IEnumerable<SelectListItem> manufacturerDropDown = _db.Manufacturer.Select(manufacturerItem => new SelectListItem
            {
                Text = manufacturerItem.Name,
                Value = manufacturerItem.Id.ToString()
            });

            IEnumerable<SelectListItem> productTypeDropDown = _db.ProductType.Select(productTypeItem => new SelectListItem
            {
                Text = productTypeItem.Name,
                Value = productTypeItem.Id.ToString()
            });

            ProductVM productVM = new()
            {
                Product = new Product(),
                ManufacturerSelectList = manufacturerDropDown,
                ProductTypeSelectList = productTypeDropDown
            };
            return View(productVM);
        }
        [Authorize(Roles = WC.AdminRole)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Product product)
        {
            if (ModelState.IsValid)
            {
                IFormFileCollection files = HttpContext.Request.Form.Files;
                string webRootPath = _webHostEnvironment.WebRootPath;
                string uploadPath = $"{ webRootPath }{ WC.ImageProductPath }";
                string extension = Path.GetExtension(files[0].FileName);

                if (extension.ToLower() == ".png" || extension.ToLower() == ".jpg" || extension.ToLower() == ".jpeg" || extension.ToLower() == ".gif")
                {
                    string imageName = $"{ Guid.NewGuid() }{ extension }";
                    using (var fileStream = new FileStream(Path.Combine(uploadPath, imageName), FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                    }
                    product.Image = imageName;

                    _db.Product.Add(product);
                    _db.SaveChanges();
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("Image", "File has to be an image");
                return Create();
            }
            return Create();
        }

        public IActionResult Edit(int? id)
        {
            if (id == 0 || id is null)
            {
                return null;
            }

            Product product = _db.Product.Find(id);
            if (product is null)
            {
                return null;
            }

            IEnumerable<SelectListItem> manufacturerDropDown = _db.Manufacturer.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            });

            IEnumerable<SelectListItem> productTypeDropDown = _db.ProductType.Select(y => new SelectListItem
            {
                Text = y.Name,
                Value = y.Id.ToString()
            });

            ProductVM productVM = new()
            {
                Product = product,
                ManufacturerSelectList = manufacturerDropDown,
                ProductTypeSelectList = productTypeDropDown
            };
            return View(productVM);
        }

        [Authorize(Roles = WC.AdminRole)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Product product)
        {
            if (ModelState.IsValid)
            {
                Product oldProduct = _db.Product.AsNoTracking().FirstOrDefault(x => x.Id == product.Id);
                product.Image = oldProduct.Image;
                IFormFileCollection files = HttpContext.Request.Form.Files;
                if (files.Count > 0)
                {
                    string webRootPath = _webHostEnvironment.WebRootPath;
                    string uploadPath = $"{ webRootPath }{ WC.ImageProductPath }";
                    string extension = Path.GetExtension(files[0].FileName);

                    if (extension.ToLower() == ".png" || extension.ToLower() == ".jpg" || extension.ToLower() == ".jpeg" || extension.ToLower() == ".gif")
                    {
                        string oldFilePath = Path.Combine(uploadPath, product.Image);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }

                        string imageName = $"{ Guid.NewGuid() }{ extension }";
                        using (var fileStream = new FileStream(Path.Combine(uploadPath, imageName), FileMode.Create))
                        {
                            files[0].CopyTo(fileStream);
                        }
                        product.Image = imageName;

                        _db.Product.Update(product);
                        _db.SaveChanges();
                        return RedirectToAction(nameof(Index));
                    }
                    ModelState.AddModelError("Image", "File has to be an image");
                    return Edit(product.Id);
                }
                _db.Product.Update(product);
                _db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return Edit(product.Id);
        }

        public IActionResult Delete(int? id)
        {
            if (id == 0 || id is null)
            {
                return null;
            }

            Product product = _db.Product.Find(id);
            if (product is null)
            {
                return null;
            }

            IEnumerable<SelectListItem> manufacturerDropDown = _db.Manufacturer.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            });

            IEnumerable<SelectListItem> productTypeDropDown = _db.ProductType.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            });

            ProductVM productVM = new()
            {
                Product = product,
                ManufacturerSelectList = manufacturerDropDown,
                ProductTypeSelectList = productTypeDropDown
            };
            return View(productVM);
        }

        [Authorize(Roles = WC.AdminRole)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirm(ProductVM productVM)
        {
            Product product = _db.Product.Find(productVM.Product.Id);
            if (product == null)
            {
                return NotFound();
            }

            string webRootPath = _webHostEnvironment.WebRootPath;
            string uploadPath = $"{ webRootPath }{ WC.ImageProductPath }";
            string oldFilePath = Path.Combine(uploadPath, product.Image);
            if (System.IO.File.Exists(oldFilePath))
            {
                System.IO.File.Delete(oldFilePath);
            }

            _db.Product.Remove(product);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
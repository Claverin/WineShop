using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WineShop.Data;
using WineShop.Models.ViewModels;
using WineShop.Utility;

namespace WineShop.Controllers;

[Authorize]
public class CartController : Controller
{
    private readonly ApplicationDbContext _db;

    [BindProperty]
    public ProductUserVM ProductUserVM { get; set; } = default!;

    public CartController(ApplicationDbContext db)
    {
        _db = db;
    }

    public IActionResult Index()
    {
        var productIds = CartSession.GetProductIds(HttpContext.Session);

        var productList = _db.Product
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToList();

        return View(productList);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("Index")]
    public IActionResult IndexPost()
        => RedirectToAction(nameof(Summary));

    public IActionResult Summary()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Challenge();

        var productIds = CartSession.GetProductIds(HttpContext.Session);

        var productList = _db.Product
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToList();

        ProductUserVM = new ProductUserVM
        {
            ApplicationUser = _db.ApplicationUser.FirstOrDefault(u => u.Id == userId),
            ProductList = productList
        };

        return View(ProductUserVM);
    }

    public IActionResult Remove(int id)
    {
        CartSession.Remove(HttpContext.Session, id);
        return RedirectToAction(nameof(Index));
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using WineShop.Data;
using WineShop.Models;
using WineShop.Models.ViewModels;
using WineShop.Utility;

namespace WineShop.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _db;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    public IActionResult Index() => View();

    public IActionResult ShopSite()
    {
        var homeVM = new HomeVM
        {
            Products = _db.Product
                .AsNoTracking()
                .Include(p => p.ProductType)
                .Include(p => p.Rating)
                .Include(p => p.Manufacturer)
                .ToList(),
            ProductTypes = _db.ProductType
                .AsNoTracking()
                .ToList()
        };

        return View(homeVM);
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await _db.Product
            .AsNoTracking()
            .Include(p => p.ProductType)
            .Include(p => p.Manufacturer)
            .Include(p => p.Comment).ThenInclude(c => c.ApplicationUser)
            .Include(p => p.Rating).ThenInclude(r => r.ApplicationUser)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null)
            return NotFound();

        product.Comment ??= new List<Comment>();
        product.Rating ??= new List<Rating>();

        var vm = new DetailsVM
        {
            Product = product,
            ExistsInCart = CartSession.Contains(HttpContext.Session, id),
            UserRating = 0
        };

        if (User.IsInRole(WC.CustomerRole) || User.IsInRole(WC.AdminRole))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                vm.UserRating = await _db.Rating
                    .AsNoTracking()
                    .Where(r => r.IdCustomer == userId && r.IdProduct == id)
                    .Select(r => r.RatingValue)
                    .FirstOrDefaultAsync();
            }
        }

        return View(vm);
    }

    [HttpPost, ActionName("Details")]
    [ValidateAntiForgeryToken]
    public IActionResult DetailsPost(int id)
    {
        CartSession.Add(HttpContext.Session, id);
        return RedirectToAction(nameof(ShopSite));
    }

    public IActionResult RemoveFromCart(int id)
    {
        CartSession.Remove(HttpContext.Session, id);
        return RedirectToAction(nameof(ShopSite));
    }

    [Authorize(Roles = WC.AdminRole + "," + WC.CustomerRole)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(Comment comment)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Forbid();

        if (comment.IdProduct is null)
            return BadRequest();

        if (!ModelState.IsValid)
            return RedirectToAction(nameof(Details), new { id = comment.IdProduct });

        var newComment = new Comment
        {
            Date = DateTime.UtcNow,
            CommentContent = comment.CommentContent,
            IdCustomer = userId,
            IdProduct = comment.IdProduct
        };

        _db.Comment.Add(newComment);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = comment.IdProduct });
    }

    [Authorize(Roles = WC.AdminRole + "," + WC.CustomerRole)]
    public async Task<IActionResult> DeleteComment(int id)
    {
        var comment = await _db.Comment.FirstOrDefaultAsync(c => c.Id == id);
        if (comment is null)
            return NotFound();

        if (!User.IsInRole(WC.AdminRole))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || userId != comment.IdCustomer)
                return Forbid();
        }

        _db.Comment.Remove(comment);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = comment.IdProduct });
    }

    [Authorize(Roles = WC.AdminRole + "," + WC.CustomerRole)]
    public async Task<IActionResult> RateProduct(int id, int rate)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Forbid();

        if (rate < 0 || rate > 5)
            return BadRequest();

        var existing = await _db.Rating
            .FirstOrDefaultAsync(r => r.IdCustomer == userId && r.IdProduct == id);

        if (rate == 0)
        {
            if (existing is not null)
            {
                _db.Rating.Remove(existing);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        if (existing is null)
        {
            _db.Rating.Add(new Rating
            {
                IdProduct = id,
                RatingValue = rate,
                IdCustomer = userId
            });
        }
        else
        {
            existing.RatingValue = rate;
            _db.Rating.Update(existing);
        }

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
        => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
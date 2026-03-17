using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using WineShop.Data;
using WineShop.Models;
using WineShop.Models.ViewModels;
using WineShop.Services.Interfaces;

namespace WineShop.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _db;
    private readonly ICartService _cartService;
    private readonly IRatingService _ratingService;
    private readonly ICommentService _commentService;
    private readonly IProductDetailsService _productDetailsService;

    public HomeController(
    ILogger<HomeController> logger,
    ApplicationDbContext db,
    ICartService cartService,
    IRatingService ratingService,
    ICommentService commentService,
    IProductDetailsService productDetailsService)
    {
        _logger = logger;
        _db = db;
        _cartService = cartService;
        _ratingService = ratingService;
        _commentService = commentService;
        _productDetailsService = productDetailsService;
    }

    public IActionResult Index() => View();

    public IActionResult ShopSite(int? typeId, int page = 1, int pageSize = 12)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 60);

        var query = _db.Product
            .AsNoTracking()
            .Include(p => p.ProductType)
            .Include(p => p.Rating)
            .Include(p => p.Manufacturer)
            .AsQueryable();

        if (typeId.HasValue)
            query = query.Where(p => p.IdProductType == typeId.Value);

        var total = query.Count();

        var products = query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var vm = new HomeVM
        {
            Products = products,
            ProductTypes = _db.ProductType.AsNoTracking().ToList(),

            Page = page,
            PageSize = pageSize,
            TotalItems = total,
            TypeId = typeId
        };

        return View(vm);
    }

    public async Task<IActionResult> Details(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var detailsVM = await _productDetailsService.GetAsync(id, userId);

        if (detailsVM is null)
        {
            return NotFound();
        }

        return View(detailsVM);
    }

    [HttpPost, ActionName("Details")]
    [ValidateAntiForgeryToken]
    public IActionResult DetailsPost(int id, int quantity)
    {
        _cartService.Add(id, Math.Max(1, quantity));
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RemoveFromCart(int id)
    {
        _cartService.Remove(id);
        return RedirectToAction(nameof(ShopSite));
    }

    [Authorize(Roles = WC.AdminRole + "," + WC.CustomerRole)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(Comment comment)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Forbid();
        }

        if (comment.IdProduct is null)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return RedirectToAction(nameof(Details), new { id = comment.IdProduct });
        }

        var productId = await _commentService.AddAsync(
            userId,
            comment.IdProduct.Value,
            comment.CommentContent ?? string.Empty);

        return RedirectToAction(nameof(Details), new { id = productId });
    }

    [Authorize(Roles = WC.AdminRole + "," + WC.CustomerRole)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteComment(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        var result = await _commentService.DeleteAsync(id, userId, User.IsInRole(WC.AdminRole));

        if (result.Status == DeleteCommentStatus.NotFound)
        {
            return NotFound();
        }

        if (result.Status == DeleteCommentStatus.Forbidden)
        {
            return Forbid();
        }

        if (result.ProductId is null)
        {
            return RedirectToAction(nameof(ShopSite));
        }

        return RedirectToAction(nameof(Details), new { id = result.ProductId });
    }

    [Authorize(Roles = WC.AdminRole + "," + WC.CustomerRole)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RateProduct(int id, int rate)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Forbid();
        }

        if (rate < 0 || rate > 5)
        {
            return BadRequest();
        }

        await _ratingService.SetRatingAsync(userId, id, rate);
        return Ok();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
        => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WineShop.Data;
using WineShop.Models;
using WineShop.Models.ViewModels;
using WineShop.Utility;

namespace WineShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _db = db;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ShopSite()
        {
            HomeVM homeVM = new HomeVM()
            {
                Products = _db.Product
                .Include(u => u.ProductType)
                .Include(u => u.Rating)
                .Include(u => u.Manufacturer),
                ProductTypes = _db.ProductType
            };
            return View(homeVM);
        }

        public async Task<IActionResult> Details(int id)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }

            DetailsVM DetailsVM = new DetailsVM()
            {
                Product = _db.Product
                .Include(u => u.ProductType)
                .Include(u => u.Manufacturer)
                .Include(u => u.Comment)
                .Include(u => u.Rating)
                .ThenInclude(u => u.ApplicationUser)
                .Where(u => u.Id == id)
                .FirstOrDefault(),
                ExistsInCart = false
            };

            if (User.IsInRole(WC.CustomerRole) || User.IsInRole(WC.AdminRole))
            {
                var userName = HttpContext.User.Identity.Name;
                var user = await _userManager.FindByNameAsync(userName);

                if (_db.Rating.Any(x => x.IdCustomer == user.Id && x.IdProduct == id))
                {
                    DetailsVM.UserRating = _db.Rating.Where(x => x.IdCustomer == user.Id && x.IdProduct == id).FirstOrDefault().RatingValue;
                }
            }

            foreach (var item in shoppingCartList)
            {
                if (item.ProductId == id)
                {
                    DetailsVM.ExistsInCart = true;
                }
            }

            return View(DetailsVM);
        }

        [HttpPost, ActionName("Details")]
        public IActionResult DetailsPost(int id)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }
            shoppingCartList.Add(new ShoppingCart { ProductId = id });
            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
            return RedirectToAction(nameof(ShopSite));
        }

        public IActionResult RemoveFromCart(int id)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }

            var itemToRemove = shoppingCartList.SingleOrDefault(r => r.ProductId == id);
            if (itemToRemove != null)
            {
                shoppingCartList.Remove(itemToRemove);
            }

            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
            return RedirectToAction(nameof(ShopSite));
        }

        [Authorize(Roles = WC.AdminRole + "," + WC.CustomerRole)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(Comment comment)
        {
            comment.Date = DateTime.Now;
            if (ModelState.IsValid)
            {
                _db.Comment.Add(comment);
                _db.SaveChanges();
            }
            return RedirectToAction(nameof(Details), new { id = comment.IdProduct });
        }

        [Authorize(Roles = WC.AdminRole)]
        public IActionResult DeleteComment(int id)
        {
            var searchingComment = _db.Comment.Find(id);
            if (searchingComment == null)
            {
                return NotFound();
            }

            _db.Comment.Remove(searchingComment);
            _db.SaveChanges();
            return RedirectToAction(nameof(Details), new { id = searchingComment.IdProduct });
        }
        
        [Authorize(Roles = WC.AdminRole + "," + WC.CustomerRole)]
        public async Task<IActionResult> RateProduct(int id, int rate)
        {
            var userName = HttpContext.User.Identity.Name;
            var user = await _userManager.FindByNameAsync(userName);
            if (rate == 0)
            {
                var productRating = _db.Rating.Where(x => x.IdCustomer == user.Id && x.IdProduct == id).FirstOrDefault();
                _db.Rating.Remove(productRating);
            }
            else if (_db.Rating.Any(x => x.IdCustomer == user.Id && x.IdProduct == id))
            {
                var productRating = _db.Rating.Where(x => x.IdCustomer == user.Id && x.IdProduct == id).FirstOrDefault();
                productRating.RatingValue = rate;
                _db.Rating.Update(productRating);
            }
            else
            {
                var productRating = new Rating()
                {
                    IdProduct = id,
                    RatingValue = rate,
                    IdCustomer = user.Id,
                };

                _db.Rating.Add(productRating);
            }
            _db.SaveChanges();
            return RedirectToAction(nameof(Details), new { id });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
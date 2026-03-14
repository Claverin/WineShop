using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WineShop.Data;
using WineShop.Models;
using WineShop.Models.ViewModels;
using WineShop.Services.Interfaces;

namespace WineShop.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _db;
        private readonly ICartService _cartService;

        public OrderService(ApplicationDbContext db, ICartService cartService)
        {
            _db = db;
            _cartService = cartService;
        }

        public async Task<CheckoutVM> BuildCheckoutAsync(string userId)
        {
            var productIds = _cartService.GetProductIds();

            var products = await _db.Product
                .AsNoTracking()
                .Where(x => productIds.Contains(x.Id))
                .OrderBy(x => x.Name)
                .ToListAsync();

            var user = await _db.ApplicationUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == userId);

            var paymentMethods = await _db.PaymentMethod
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name
                })
                .ToListAsync();

            var fullName = string.Join(" ", new[] { user?.Name, user?.Surname }.Where(x => !string.IsNullOrWhiteSpace(x))).Trim();

            if (string.IsNullOrWhiteSpace(fullName))
            {
                fullName = user?.UserName ?? string.Empty;
            }

            var defaultPaymentMethodId = paymentMethods
                .Select(x => int.Parse(x.Value))
                .FirstOrDefault();

            return new CheckoutVM
            {
                Name = fullName,
                Email = user?.Email ?? string.Empty,
                PhoneNumber = user?.PhoneNumber ?? string.Empty,
                Street = string.Empty,
                PostalCode = string.Empty,
                City = string.Empty,
                PaymentMethodId = defaultPaymentMethodId,
                PaymentMethods = paymentMethods,
                Items = products.Select(x => new CheckoutItemVM
                {
                    ProductId = x.Id,
                    Name = x.Name,
                    Price = x.Price
                }).ToList()
            };
        }

        public async Task<int?> PlaceOrderAsync(CheckoutVM model, string userId)
        {
            var productIds = _cartService.GetProductIds();

            if (!productIds.Any())
            {
                return null;
            }

            var paymentMethodExists = await _db.PaymentMethod
                .AsNoTracking()
                .AnyAsync(x => x.Id == model.PaymentMethodId);

            if (!paymentMethodExists)
            {
                return null;
            }

            var pendingStatusId = await _db.OrderStatus
                .AsNoTracking()
                .Where(x => x.Name == "Pending")
                .Select(x => x.Id)
                .FirstOrDefaultAsync();

            if (pendingStatusId == 0)
            {
                return null;
            }

            var products = await _db.Product
                .AsNoTracking()
                .Where(x => productIds.Contains(x.Id))
                .OrderBy(x => x.Name)
                .ToListAsync();

            if (!products.Any())
            {
                return null;
            }

            var items = products.Select(x => new OrderItem
            {
                ProductId = x.Id,
                ProductName = x.Name,
                UnitPrice = x.Price,
                Quantity = 1
            }).ToList();

            var order = new Order
            {
                CreatedAtUtc = DateTime.UtcNow,
                CustomerId = userId,
                CustomerName = model.Name,
                CustomerEmail = model.Email,
                CustomerPhoneNumber = model.PhoneNumber,
                Street = model.Street,
                PostalCode = model.PostalCode,
                City = model.City,
                PaymentMethodId = model.PaymentMethodId,
                OrderStatusId = pendingStatusId,
                TotalAmount = items.Sum(x => x.UnitPrice * x.Quantity),
                Items = items
            };

            _db.Order.Add(order);
            await _db.SaveChangesAsync();

            _cartService.Clear();

            return order.Id;
        }

        public async Task<OrderConfirmationVM?> GetConfirmationAsync(int orderId, string userId, bool isAdmin)
        {
            var order = await _db.Order
                .AsNoTracking()
                .Include(x => x.OrderStatus)
                .FirstOrDefaultAsync(x => x.Id == orderId);

            if (order is null)
            {
                return null;
            }

            if (!isAdmin && order.CustomerId != userId)
            {
                return null;
            }

            return new OrderConfirmationVM
            {
                OrderId = order.Id,
                CreatedAtUtc = order.CreatedAtUtc,
                TotalAmount = order.TotalAmount,
                StatusName = order.OrderStatus.Name
            };
        }
    }
}
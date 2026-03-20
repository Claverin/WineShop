using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WineShop.Data;
using WineShop.Models.ViewModels;
using WineShop.Services.Interfaces;

namespace WineShop.Services
{
    public class AdminOrderService : IAdminOrderService
    {
        private readonly ApplicationDbContext _db;

        public AdminOrderService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<AdminOrderListItemVM>> GetOrdersAsync()
        {
            return await _db.Order
                .AsNoTracking()
                .Include(x => x.OrderStatus)
                .Include(x => x.PaymentMethod)
                .OrderByDescending(x => x.CreatedAtUtc)
                .Select(x => new AdminOrderListItemVM
                {
                    OrderId = x.Id,
                    CustomerEmail = x.CustomerEmail,
                    CustomerName = x.CustomerName,
                    StatusName = x.OrderStatus.Name,
                    PaymentMethodName = x.PaymentMethod.Name,
                    TotalAmount = x.TotalAmount,
                    CreatedAtUtc = x.CreatedAtUtc,
                    HasShippingInfo =
                        !string.IsNullOrWhiteSpace(x.Carrier) ||
                        !string.IsNullOrWhiteSpace(x.ShippingMethod) ||
                        !string.IsNullOrWhiteSpace(x.TrackingNumber) ||
                        x.ShippedDate != null ||
                        x.DeliveredDate != null
                })
                .ToListAsync();
        }

        public async Task<AdminOrderDetailsVM?> GetOrderAsync(int orderId)
        {
            var order = await _db.Order
                .AsNoTracking()
                .Include(x => x.OrderStatus)
                .Include(x => x.PaymentMethod)
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == orderId);

            if (order is null)
            {
                return null;
            }

            var statuses = await _db.OrderStatus
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name
                })
                .ToListAsync();

            return new AdminOrderDetailsVM
            {
                OrderId = order.Id,
                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail,
                CustomerPhoneNumber = order.CustomerPhoneNumber,
                Street = order.Street,
                PostalCode = order.PostalCode,
                City = order.City,
                TotalAmount = order.TotalAmount,
                CreatedAtUtc = order.CreatedAtUtc,
                OrderStatusId = order.OrderStatusId,
                Carrier = order.Carrier,
                ShippingMethod = order.ShippingMethod,
                TrackingNumber = order.TrackingNumber,
                ShippedDate = order.ShippedDate,
                DeliveredDate = order.DeliveredDate,
                ShippingNotes = order.ShippingNotes,
                CurrentStatusName = order.OrderStatus.Name,
                PaymentMethodName = order.PaymentMethod.Name,
                Statuses = statuses,
                Items = order.Items
                    .OrderBy(x => x.ProductName)
                    .Select(x => new AdminOrderItemVM
                    {
                        ProductId = x.ProductId,
                        ProductName = x.ProductName,
                        UnitPrice = x.UnitPrice,
                        Quantity = x.Quantity
                    })
                    .ToList()
            };
        }

        public async Task<bool> UpdateOrderAsync(AdminOrderDetailsVM model)
        {
            var order = await _db.Order.FirstOrDefaultAsync(x => x.Id == model.OrderId);

            if (order is null)
            {
                return false;
            }

            var statusExists = await _db.OrderStatus.AnyAsync(x => x.Id == model.OrderStatusId);

            if (!statusExists)
            {
                return false;
            }

            order.OrderStatusId = model.OrderStatusId;
            order.Carrier = string.IsNullOrWhiteSpace(model.Carrier) ? null : model.Carrier.Trim();
            order.ShippingMethod = string.IsNullOrWhiteSpace(model.ShippingMethod) ? null : model.ShippingMethod.Trim();
            order.TrackingNumber = string.IsNullOrWhiteSpace(model.TrackingNumber) ? null : model.TrackingNumber.Trim();
            order.ShippedDate = model.ShippedDate;
            order.DeliveredDate = model.DeliveredDate;
            order.ShippingNotes = string.IsNullOrWhiteSpace(model.ShippingNotes) ? null : model.ShippingNotes.Trim();

            _db.Order.Update(order);
            await _db.SaveChangesAsync();

            return true;
        }
    }
}
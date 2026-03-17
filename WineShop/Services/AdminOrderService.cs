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
                    HasShipment = x.ShipmentId != null
                })
                .ToListAsync();
        }

        public async Task<AdminOrderDetailsVM?> GetOrderAsync(int orderId)
        {
            var order = await _db.Order
                .AsNoTracking()
                .Include(x => x.OrderStatus)
                .Include(x => x.PaymentMethod)
                .Include(x => x.Shipment)
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

            var shipments = await _db.Shipment
                .AsNoTracking()
                .OrderByDescending(x => x.SendDate)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = $"#{x.Id} | {x.SendDate:yyyy-MM-dd} -> {x.DeliverDate:yyyy-MM-dd}"
                })
                .ToListAsync();

            shipments.Insert(0, new SelectListItem
            {
                Value = "",
                Text = "No shipment"
            });

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
                ShipmentId = order.ShipmentId,
                CurrentStatusName = order.OrderStatus.Name,
                PaymentMethodName = order.PaymentMethod.Name,
                ShipmentSendDate = order.Shipment?.SendDate,
                ShipmentDeliverDate = order.Shipment?.DeliverDate,
                Statuses = statuses,
                Shipments = shipments,
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

        public async Task<bool> UpdateOrderAsync(int orderId, int orderStatusId, int? shipmentId)
        {
            var order = await _db.Order.FirstOrDefaultAsync(x => x.Id == orderId);

            if (order is null)
            {
                return false;
            }

            var statusExists = await _db.OrderStatus.AnyAsync(x => x.Id == orderStatusId);

            if (!statusExists)
            {
                return false;
            }

            if (shipmentId.HasValue)
            {
                var shipmentExists = await _db.Shipment.AnyAsync(x => x.Id == shipmentId.Value);

                if (!shipmentExists)
                {
                    return false;
                }
            }

            order.OrderStatusId = orderStatusId;
            order.ShipmentId = shipmentId;

            _db.Order.Update(order);
            await _db.SaveChangesAsync();

            return true;
        }
    }
}
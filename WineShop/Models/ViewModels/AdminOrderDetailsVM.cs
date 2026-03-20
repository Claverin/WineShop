using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace WineShop.Models.ViewModels
{
    public class AdminOrderDetailsVM
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhoneNumber { get; set; }
        public string Street { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        [Display(Name = "Status")]
        public int OrderStatusId { get; set; }

        [Display(Name = "Carrier")]
        public string? Carrier { get; set; }

        [Display(Name = "Shipping Method")]
        public string? ShippingMethod { get; set; }

        [Display(Name = "Tracking Number")]
        public string? TrackingNumber { get; set; }

        [Display(Name = "Shipped Date")]
        public DateTime? ShippedDate { get; set; }

        [Display(Name = "Delivered Date")]
        public DateTime? DeliveredDate { get; set; }

        [Display(Name = "Shipping Notes")]
        public string? ShippingNotes { get; set; }

        public string CurrentStatusName { get; set; }
        public string PaymentMethodName { get; set; }

        public List<AdminOrderItemVM> Items { get; set; } = new();
        public IEnumerable<SelectListItem> Statuses { get; set; } = new List<SelectListItem>();
    }
}
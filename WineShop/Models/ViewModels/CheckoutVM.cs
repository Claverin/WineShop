using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace WineShop.Models.ViewModels
{
    public class CheckoutVM
    {
        [Required]
        [StringLength(150)]
        public string Name { get; set; }

        [Required]
        [Phone]
        [Display(Name = "Phone")]
        public string PhoneNumber { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(150)]
        public string Street { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; }

        [Required]
        [StringLength(100)]
        public string City { get; set; }

        [Required]
        [Display(Name = "Payment Method")]
        public int PaymentMethodId { get; set; }

        public List<CheckoutItemVM> Items { get; set; } = new();
        public IEnumerable<SelectListItem> PaymentMethods { get; set; } = new List<SelectListItem>();

        public decimal TotalAmount => Items.Sum(x => x.Price);
    }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WineShop.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        public string CustomerId { get; set; }

        [Required]
        [StringLength(150)]
        public string CustomerName { get; set; }

        [Required]
        [StringLength(150)]
        [EmailAddress]
        public string CustomerEmail { get; set; }

        [Required]
        [StringLength(30)]
        public string CustomerPhoneNumber { get; set; }

        [Required]
        [StringLength(150)]
        public string Street { get; set; }

        [Required]
        [StringLength(20)]
        public string PostalCode { get; set; }

        [Required]
        [StringLength(100)]
        public string City { get; set; }

        [Required]
        public int PaymentMethodId { get; set; }

        [ForeignKey(nameof(PaymentMethodId))]
        public PaymentMethod PaymentMethod { get; set; }

        [Required]
        public int OrderStatusId { get; set; }

        [ForeignKey(nameof(OrderStatusId))]
        public OrderStatus OrderStatus { get; set; }

        public int? ShipmentId { get; set; }

        [ForeignKey(nameof(ShipmentId))]
        public Shipment? Shipment { get; set; }

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
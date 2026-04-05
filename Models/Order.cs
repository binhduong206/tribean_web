// Models/Order.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tribean.Models.Enums;

namespace Tribean.Models
{
    public class Order
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;

        [Required]
        public string UserId { get; set; } = string.Empty;

        public string? OrderDiscountId { get; set; }

        [MaxLength(100)]
        public string? ReceiverName { get; set; }

        [MaxLength(300)]
        public string? ReceiverAddress { get; set; }

        [MaxLength(20)]
        public string? ReceiverPhoneNumber { get; set; }

        // Navigation
        [ForeignKey("UserId")]
        public User? User { get; set; }

        [ForeignKey("OrderDiscountId")]
        public Discount? Discount { get; set; }

        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
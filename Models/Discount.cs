// Models/Discount.cs
using System.ComponentModel.DataAnnotations;

namespace Tribean.Models
{
    public class Discount
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required, MaxLength(100)]
        public string DiscountName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? DiscountType { get; set; } // "percent" | "fixed"

        public int DiscountValue { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsAvailable { get; set; } = true;

        // Navigation
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
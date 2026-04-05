// Models/Size.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tribean.Models
{
    public class Size
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required, MaxLength(10)]
        public string SizeName { get; set; } = string.Empty; // "S", "M", "L"

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } // Phụ phí theo size

        // Navigation
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
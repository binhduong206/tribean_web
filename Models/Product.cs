// Models/Product.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation; // Thư viện bắt buộc để dùng [ValidateNever]

namespace Tribean.Models
{
    public class Product
    {
        [Key]
        [ValidateNever] // Bỏ qua validate trường Id khi submit form (vì hệ thống tự sinh ra)
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string? MainImgUrl { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        [MaxLength(200)]
        public string ProductName { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        public string CategoryId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool Status { get; set; } = true;

        [Column(TypeName = "decimal(18,2)")]
        [Required(ErrorMessage = "Vui lòng nhập giá tiền")]
        public decimal Price { get; set; }

        [Range(0, 100, ErrorMessage = "Giảm giá chỉ từ 0% đến 100%")]
        public int Discount { get; set; } = 0; // Mặc định là 0%

        // ==========================================
        // NAVIGATION PROPERTIES (Phải có ValidateNever)
        // ==========================================

        [ForeignKey("CategoryId")]
        [ValidateNever] // Tránh lỗi đòi validate object Category
        public Category? Category { get; set; }

        [ValidateNever] // Tránh lỗi đòi validate mảng Images
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();

        [ValidateNever] // Tránh lỗi đòi validate mảng OrderDetails
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

        [ValidateNever] // Tránh lỗi đòi validate mảng CartItems
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

        [ValidateNever] // Tránh lỗi đòi validate mảng Reviews
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
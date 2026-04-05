// Models/Category.cs
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Tribean.Models
{
    public class Category
    {
        [Key]
        [ValidateNever] // Bỏ qua bắt lỗi Id
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required(ErrorMessage = "Vui lòng nhập tên danh mục")]
        [MaxLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        // Navigation
        [ValidateNever] // Cực kỳ quan trọng: Bỏ qua bắt lỗi mảng Products
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
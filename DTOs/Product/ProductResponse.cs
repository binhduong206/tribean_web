using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Tribean.Models;

namespace Tribean.DTOs;

public class ProductResponse
{
    public string? MainImgUrl { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string CategoryId { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int? Discount { get; set; } = null; // Mặc định là 0%
    public string? CategoryName { get; set; }
}
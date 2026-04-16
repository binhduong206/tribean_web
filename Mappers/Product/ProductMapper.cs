using System.Linq.Expressions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Tribean.DTOs;
using Tribean.Models;

namespace Tribean.Mappers;

public static class ProductMapper
{
    public static string baseUrl = "http://localhost:5262";
    public static Expression<Func<Product, ProductHomeResponse>> ToHomeDto()
    {
        return p => new ProductHomeResponse
        {
            Id = p.Id,
            ProductName = p.ProductName,
            CategoryName = p.Category!.CategoryName,
            Price = p.Price,
            Discount = p.Discount,
            Description = p.Description,
            MainImgUrl = string.IsNullOrEmpty(p.MainImgUrl)
                ? null
                : baseUrl + p.MainImgUrl,
            Rating = p.Reviews
                .Select(r => (double?)r.Rating)
                .Average() ?? 0
        };
    }
}
using Tribean.Data;
using Tribean.DTOs;
using Tribean.Mappers;
using Tribean.Models;
using Microsoft.EntityFrameworkCore;
using Tribean.Helpers;

namespace Tribean.Repositories;

public class HomeRepository : IHomeRepository
{
    private readonly ApplicationDbContext _context;
    public HomeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductHomeResponse>> GetFeaturedProductAsync(QueryObject query)
    {
        var products = _context.Products.Where(p => p.Status).Select(ProductMapper.ToHomeDto());

        var skipNumber = (query.PageNumber - 1) * query.PageSizeBestSeller;

        return await products.Skip(skipNumber).Take(query.PageSizeBestSeller).ToListAsync();
        // return await _context.Products
        //     .Where(p => p.Status)
        //     .OrderByDescending(p => p.CreatedAt)
        //     .Take(8)
        //     .Select(ProductMapper.ToHomeDto())
        //     .ToListAsync();
    }
}
using Tribean.DTOs;
using Tribean.Helpers;
using Tribean.Repositories;

namespace Tribean.Services;

public class HomeService : IHomeService
{
    private readonly IHomeRepository _repo;
    public HomeService(IHomeRepository repo)
    {
        _repo = repo;
    }
    public async Task<List<ProductHomeResponse>> GetHomeDataAsync(QueryObject query)
    {
        var products = await _repo.GetFeaturedProductAsync(query);
        return products;
    }
}
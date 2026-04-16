using Tribean.DTOs;
using Tribean.Helpers;

namespace Tribean.Repositories;

public interface IHomeRepository
{
    Task<List<ProductHomeResponse>> GetFeaturedProductAsync(QueryObject query);
}
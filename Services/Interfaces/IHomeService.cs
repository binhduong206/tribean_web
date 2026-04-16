using Tribean.DTOs;
using Tribean.Helpers;

namespace Tribean.Services;

public interface IHomeService
{
    Task<List<ProductHomeResponse>> GetHomeDataAsync(QueryObject query);
}
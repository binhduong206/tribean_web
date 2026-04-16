using Microsoft.AspNetCore.Mvc;
using Tribean.Helpers;
using Tribean.Services;

namespace Tribean.Controllers;

[Route("api/home")]
[ApiController]
public class HomeController : ControllerBase
{
    private readonly IHomeService _homeService;
    public HomeController(IHomeService homeService)
    {
        _homeService = homeService;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] QueryObject query)
    {
        var data = await _homeService.GetHomeDataAsync(query);
        return Ok(data);
    }
}
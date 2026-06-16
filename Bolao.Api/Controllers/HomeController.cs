using Bolao.Application.Home.GetHome;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Bolao.Api.Controllers;

[ApiController]
[Route("api/home")]
[Authorize]
public class HomeController : ControllerBase
{
    private readonly GetHomeUseCase _getHomeUseCase;

    public HomeController(
        GetHomeUseCase getHomeUseCase)
    {
        _getHomeUseCase = getHomeUseCase;
    }

    [HttpGet]
    [EnableRateLimiting("General")]
    public async Task<IActionResult> Get(
        CancellationToken cancellationToken)
    {
        var result = await _getHomeUseCase.ExecuteAsync(
            new GetHomeQuery(),
            cancellationToken);

        return Ok(result);
    }
}
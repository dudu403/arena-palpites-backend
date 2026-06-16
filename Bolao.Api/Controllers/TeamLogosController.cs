using Bolao.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bolao.Api.Controllers;

[ApiController]
[Route("api/team-logos")]
public sealed class TeamLogosController : ControllerBase
{
    private readonly ITeamLogoService _teamLogoService;

    public TeamLogosController(ITeamLogoService teamLogoService)
    {
        _teamLogoService = teamLogoService;
    }

    [HttpGet("{externalTeamId:int}")]
    public async Task<IActionResult> Get(
        int externalTeamId,
        CancellationToken cancellationToken)
    {
        var logo = await _teamLogoService.GetLogoAsync(
            externalTeamId,
            cancellationToken);

        if (logo is null)
            return NotFound();

        Response.Headers.CacheControl = "no-store, no-cache, must-revalidate";
        Response.Headers.Pragma = "no-cache";
        Response.Headers.Expires = "0";

        return File(
            logo.Content,
            logo.ContentType);
    }
}
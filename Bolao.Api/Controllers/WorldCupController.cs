using Bolao.Application.WorldCup.GetWorldCupMatches;
using Bolao.Application.WorldCup.SyncWorldCup;
using Bolao.Infrastructure.ExternalServices.ApiFutebol;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bolao.Api.Controllers;

[ApiController]
[Route("api/worldcup")]
[Authorize]
public class WorldCupController : ControllerBase
{
    private readonly IApiFutebolService _apiFutebol;
    private readonly SyncWorldCupUseCase _syncWorldCupUseCase;
    private readonly GetWorldCupMatchesUseCase _getWorldCupMatchesUseCase;

    public WorldCupController(
        IApiFutebolService apiFutebol,
        SyncWorldCupUseCase syncWorldCupUseCase,
        GetWorldCupMatchesUseCase getWorldCupMatchesUseCase)
    {
        _apiFutebol = apiFutebol;
        _syncWorldCupUseCase = syncWorldCupUseCase;
        _getWorldCupMatchesUseCase = getWorldCupMatchesUseCase;
    }

    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Get()
    {
        var result = await _apiFutebol.GetWorldCupAsync();

        return Content(result, "application/json");
    }

    [HttpGet("groups")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetGroups()
    {
        var result = await _apiFutebol.GetWorldCupGroupStageAsync();

        return Content(result, "application/json");
    }

    [HttpGet("matches")]
    public async Task<IActionResult> GetMatches(
        [FromQuery] int? roundNumber,
        [FromQuery] string? groupSlug,
        CancellationToken cancellationToken)
    {
        var query = new GetWorldCupMatchesQuery
        {
            RoundNumber = roundNumber,
            GroupSlug = groupSlug
        };

        var result = await _getWorldCupMatchesUseCase.ExecuteAsync(
            query,
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("sync")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Sync(
        CancellationToken cancellationToken)
    {
        var command = new SyncWorldCupCommand();

        var result = await _syncWorldCupUseCase.ExecuteAsync(
            command,
            cancellationToken);

        return Ok(result);
    }
}
using Bolao.Application.Matches.GetMatchDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Bolao.Api.Controllers;

[ApiController]
[Route("api/matches")]
[Authorize]
public class MatchesController : ControllerBase
{
    private readonly GetMatchDetailsUseCase _getMatchDetailsUseCase;

    public MatchesController(GetMatchDetailsUseCase getMatchDetailsUseCase)
    {
        _getMatchDetailsUseCase = getMatchDetailsUseCase;
    }

    [HttpGet("{id:guid}")]
    [EnableRateLimiting("General")]
    public async Task<IActionResult> GetById(
        Guid id,
        [FromQuery] Guid bolaoId,
        CancellationToken cancellationToken)
    {
        var query = new GetMatchDetailsQuery
        {
            MatchId = id,
            BolaoId = bolaoId
        };

        var result = await _getMatchDetailsUseCase.ExecuteAsync(
            query,
            cancellationToken);

        return Ok(result);
    }
}
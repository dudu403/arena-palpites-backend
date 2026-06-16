using Bolao.Api.Contracts.Predictions;
using Bolao.Application.Predictions.CreatePrediction;
using Bolao.Application.Predictions.GetMyPredictions;
using Bolao.Application.Predictions.GetPredictionHistory;
using Bolao.Application.Predictions.ProcessFinishedMatches;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Bolao.Api.Controllers;

[ApiController]
[Route("api/predictions")]
[Authorize]
public class PredictionsController : ControllerBase
{
    private readonly CreatePredictionUseCase _createPredictionUseCase;
    private readonly GetMyPredictionsUseCase _getMyPredictionsUseCase;
    private readonly GetPredictionHistoryUseCase _getPredictionHistoryUseCase;
    private readonly ProcessFinishedMatchesUseCase _processFinishedMatchesUseCase;

    public PredictionsController(
        CreatePredictionUseCase createPredictionUseCase,
        GetMyPredictionsUseCase getMyPredictionsUseCase,
        GetPredictionHistoryUseCase getPredictionHistoryUseCase,
        ProcessFinishedMatchesUseCase processFinishedMatchesUseCase)
    {
        _createPredictionUseCase = createPredictionUseCase;
        _getMyPredictionsUseCase = getMyPredictionsUseCase;
        _getPredictionHistoryUseCase = getPredictionHistoryUseCase;
        _processFinishedMatchesUseCase = processFinishedMatchesUseCase;
    }

    [HttpPost]
    [EnableRateLimiting("Sensitive")]
    public async Task<IActionResult> Create(
        [FromBody] CreatePredictionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreatePredictionCommand
        {
            BolaoId = request.BolaoId,
            FootballMatchId = request.FootballMatchId,
            HomeScore = request.HomeScore,
            AwayScore = request.AwayScore
        };

        var result = await _createPredictionUseCase.ExecuteAsync(
            command,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("my")]
    [EnableRateLimiting("General")]
    public async Task<IActionResult> GetMy(
        [FromQuery] Guid bolaoId,
        CancellationToken cancellationToken)
    {
        var query = new GetMyPredictionsQuery
        {
            BolaoId = bolaoId
        };

        var result = await _getMyPredictionsUseCase.ExecuteAsync(
            query,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("history")]
    [EnableRateLimiting("General")]
    public async Task<IActionResult> GetHistory(
        [FromQuery] Guid bolaoId,
        CancellationToken cancellationToken)
    {
        var query = new GetPredictionHistoryQuery
        {
            BolaoId = bolaoId
        };

        var result = await _getPredictionHistoryUseCase.ExecuteAsync(
            query,
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("process-finished-matches")]
    [Authorize(Policy = "AdminOnly")]
    [EnableRateLimiting("Sensitive")]
    public async Task<IActionResult> ProcessFinishedMatches(
        [FromQuery] int maxMatchesToProcess = 50,
        CancellationToken cancellationToken = default)
    {
        var command = new ProcessFinishedMatchesCommand
        {
            MaxMatchesToProcess = maxMatchesToProcess
        };

        var result = await _processFinishedMatchesUseCase.ExecuteAsync(
            command,
            cancellationToken);

        return Ok(result);
    }
}
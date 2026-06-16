using Bolao.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bolao.Application.Predictions.CalculatePredictionPoints;

public sealed class CalculatePredictionPointsUseCase
{
    private readonly IApplicationDbContext _context;
    private readonly IPredictionScoreCalculator _calculator;

    public CalculatePredictionPointsUseCase(
        IApplicationDbContext context,
        IPredictionScoreCalculator calculator)
    {
        _context = context;
        _calculator = calculator;
    }

    public async Task<CalculatePredictionPointsResponse> ExecuteAsync(
        CalculatePredictionPointsCommand command,
        CancellationToken cancellationToken)
    {
        var matches = await _context.FootballMatches
            .Where(x =>
                !x.PointsCalculated &&
                x.Status == "finalizado" &&
                x.HomeScore != null &&
                x.AwayScore != null)
            .OrderBy(x => x.MatchDate)
            .Take(50)
            .ToListAsync(cancellationToken);

        if (matches.Count == 0)
        {
            return new CalculatePredictionPointsResponse
            {
                MatchesProcessed = 0,
                PredictionsProcessed = 0
            };
        }

        var matchIds = matches
            .Select(x => x.Id)
            .ToList();

        var predictions = await _context.Predictions
            .Where(x => matchIds.Contains(x.FootballMatchId))
            .ToListAsync(cancellationToken);

        var bolaoIds = predictions
            .Select(x => x.BolaoId)
            .Distinct()
            .ToList();

        var rulesByBolaoId = await _context.BolaoRules
            .Where(x => bolaoIds.Contains(x.BolaoId))
            .ToDictionaryAsync(x => x.BolaoId, cancellationToken);

        var matchesById = matches.ToDictionary(x => x.Id);

        var processedPredictions = 0;

        foreach (var prediction in predictions)
        {
            if (!matchesById.TryGetValue(prediction.FootballMatchId, out var match))
                continue;

            if (!rulesByBolaoId.TryGetValue(prediction.BolaoId, out var rules))
                continue;

            var result = _calculator.Calculate(
                prediction,
                match,
                rules);

            prediction.PointsEarned = result.PointsEarned;
            prediction.ExactScoreHit = result.ExactScoreHit;
            prediction.WinnerHit = result.WinnerHit;
            prediction.UpdatedAt = DateTime.UtcNow;

            processedPredictions++;
        }

        foreach (var match in matches)
        {
            match.PointsCalculated = true;
            match.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new CalculatePredictionPointsResponse
        {
            MatchesProcessed = matches.Count,
            PredictionsProcessed = processedPredictions
        };
    }
}
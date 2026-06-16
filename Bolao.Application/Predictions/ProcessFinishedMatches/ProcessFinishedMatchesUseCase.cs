using Bolao.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bolao.Application.Predictions.ProcessFinishedMatches;

public sealed class ProcessFinishedMatchesUseCase
{
    private readonly IApplicationDbContext _context;
    private readonly IPredictionScoreCalculator _scoreCalculator;

    public ProcessFinishedMatchesUseCase(
        IApplicationDbContext context,
        IPredictionScoreCalculator scoreCalculator)
    {
        _context = context;
        _scoreCalculator = scoreCalculator;
    }

    public async Task<ProcessFinishedMatchesResponse> ExecuteAsync(
        ProcessFinishedMatchesCommand command,
        CancellationToken cancellationToken)
    {
        var maxMatches = command.MaxMatchesToProcess <= 0
            ? 50
            : command.MaxMatchesToProcess;

        var finishedMatches = await _context.FootballMatches
            .Where(match =>
                !match.PointsCalculated &&
                match.Status == "finalizado" &&
                match.HomeScore.HasValue &&
                match.AwayScore.HasValue)
            .OrderBy(match => match.MatchDate)
            .Take(maxMatches)
            .ToListAsync(cancellationToken);

        if (finishedMatches.Count == 0)
        {
            return new ProcessFinishedMatchesResponse
            {
                MatchesProcessed = 0,
                PredictionsProcessed = 0
            };
        }

        var matchIds = finishedMatches
            .Select(match => match.Id)
            .ToList();

        var predictions = await _context.Predictions
            .Where(prediction => matchIds.Contains(prediction.FootballMatchId))
            .ToListAsync(cancellationToken);

        var bolaoIds = predictions
            .Select(prediction => prediction.BolaoId)
            .Distinct()
            .ToList();

        var rulesByBolaoId = await _context.BolaoRules
            .Where(rules => bolaoIds.Contains(rules.BolaoId))
            .ToDictionaryAsync(
                rules => rules.BolaoId,
                cancellationToken);

        var matchesById = finishedMatches
            .ToDictionary(match => match.Id);

        var predictionsProcessed = 0;

        foreach (var prediction in predictions)
        {
            if (!matchesById.TryGetValue(prediction.FootballMatchId, out var match))
                continue;

            if (!rulesByBolaoId.TryGetValue(prediction.BolaoId, out var rules))
                continue;

            var scoreResult = _scoreCalculator.Calculate(
                prediction,
                match,
                rules);

            prediction.PointsEarned = scoreResult.PointsEarned;
            prediction.ExactScoreHit = scoreResult.ExactScoreHit;
            prediction.WinnerHit = scoreResult.WinnerHit;
            prediction.UpdatedAt = DateTime.UtcNow;

            predictionsProcessed++;
        }

        foreach (var match in finishedMatches)
        {
            match.PointsCalculated = true;
            match.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new ProcessFinishedMatchesResponse
        {
            MatchesProcessed = finishedMatches.Count,
            PredictionsProcessed = predictionsProcessed
        };
    }
}
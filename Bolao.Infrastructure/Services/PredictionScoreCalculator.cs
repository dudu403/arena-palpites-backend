using Bolao.Application.Common.Interfaces;
using Bolao.Application.Predictions.CalculatePredictionPoints;
using Bolao.Domain.Entities;

namespace Bolao.Infrastructure.Services;

public sealed class PredictionScoreCalculator : IPredictionScoreCalculator
{
    public PredictionScoreResult Calculate(
        Prediction prediction,
        FootballMatch match,
        BolaoRules rules)
    {
        if (match.HomeScore is null || match.AwayScore is null)
            return new PredictionScoreResult();

        var actualHome = match.HomeScore.Value;
        var actualAway = match.AwayScore.Value;

        var predictedHome = prediction.HomeScore;
        var predictedAway = prediction.AwayScore;

        var exactScoreHit =
            actualHome == predictedHome &&
            actualAway == predictedAway;

        var actualWinner = GetWinner(actualHome, actualAway);
        var predictedWinner = GetWinner(predictedHome, predictedAway);

        var drawHit =
            actualWinner == MatchWinner.Draw &&
            predictedWinner == MatchWinner.Draw;

        var winnerHit =
            actualWinner != MatchWinner.Draw &&
            actualWinner == predictedWinner;

        var result = new PredictionScoreResult
        {
            ExactScoreHit = exactScoreHit,
            WinnerHit = winnerHit
        };

        if (exactScoreHit)
        {
            result.PointsEarned = rules.ExactScorePoints;
            return result;
        }

        if (drawHit)
        {
            result.PointsEarned = rules.DrawPoints;
            return result;
        }

        if (winnerHit)
        {
            result.PointsEarned = rules.WinnerPoints;
            return result;
        }

        result.PointsEarned = 0;
        return result;
    }

    private static MatchWinner GetWinner(int home, int away)
    {
        if (home > away)
            return MatchWinner.Home;

        if (away > home)
            return MatchWinner.Away;

        return MatchWinner.Draw;
    }

    private enum MatchWinner
    {
        Home,
        Away,
        Draw
    }
}
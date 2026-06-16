namespace Bolao.Application.Predictions.CalculatePredictionPoints;

public sealed class PredictionScoreResult
{
    public int PointsEarned { get; set; }

    public bool ExactScoreHit { get; set; }

    public bool WinnerHit { get; set; }
}
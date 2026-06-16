namespace Bolao.Application.Predictions.CreatePrediction;

public sealed class CreatePredictionResponse
{
    public Guid Id { get; set; }

    public Guid BolaoId { get; set; }

    public Guid FootballMatchId { get; set; }

    public int HomeScore { get; set; }

    public int AwayScore { get; set; }

    public int PointsEarned { get; set; }
}
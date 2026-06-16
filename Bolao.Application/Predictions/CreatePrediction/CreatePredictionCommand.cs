namespace Bolao.Application.Predictions.CreatePrediction;

public sealed class CreatePredictionCommand
{
    public Guid BolaoId { get; set; }

    public Guid FootballMatchId { get; set; }

    public int HomeScore { get; set; }

    public int AwayScore { get; set; }
}
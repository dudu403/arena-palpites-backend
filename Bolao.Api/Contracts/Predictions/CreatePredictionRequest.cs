namespace Bolao.Api.Contracts.Predictions;

public sealed class CreatePredictionRequest
{
    public Guid BolaoId { get; set; }

    public Guid FootballMatchId { get; set; }

    public int HomeScore { get; set; }

    public int AwayScore { get; set; }
}
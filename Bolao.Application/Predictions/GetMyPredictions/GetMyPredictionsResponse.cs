namespace Bolao.Application.Predictions.GetMyPredictions;

public sealed class GetMyPredictionsResponse
{
    public Guid Id { get; set; }

    public Guid BolaoId { get; set; }

    public Guid FootballMatchId { get; set; }

    public int HomeScore { get; set; }

    public int AwayScore { get; set; }

    public int PointsEarned { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
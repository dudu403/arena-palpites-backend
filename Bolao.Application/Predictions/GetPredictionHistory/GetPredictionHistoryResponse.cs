namespace Bolao.Application.Predictions.GetPredictionHistory;

public sealed class GetPredictionHistoryResponse
{
    public Guid PredictionId { get; set; }

    public Guid FootballMatchId { get; set; }

    public string HomeTeamName { get; set; } = string.Empty;

    public string AwayTeamName { get; set; } = string.Empty;

    public int PredictedHomeScore { get; set; }

    public int PredictedAwayScore { get; set; }

    public int? RealHomeScore { get; set; }

    public int? RealAwayScore { get; set; }

    public string MatchStatus { get; set; } = string.Empty;

    public DateTime? MatchDate { get; set; }

    public string MatchDateText { get; set; } = string.Empty;

    public string MatchTimeText { get; set; } = string.Empty;

    public int PointsEarned { get; set; }

    public bool ExactScoreHit { get; set; }

    public bool WinnerHit { get; set; }
}
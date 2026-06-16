namespace Bolao.Application.Boloes.GetMemberFinishedPredictions;

public sealed class GetMemberFinishedPredictionsResponse
{
    public Guid UserId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string? PhotoUrl { get; set; }

    public int TotalPoints { get; set; }

    public int PredictionsCount { get; set; }

    public List<MemberFinishedPredictionMatchResponse> Matches { get; set; } = [];
}

public sealed class MemberFinishedPredictionMatchResponse
{
    public Guid PredictionId { get; set; }

    public Guid FootballMatchId { get; set; }

    public string HomeTeamName { get; set; } = string.Empty;

    public string AwayTeamName { get; set; } = string.Empty;

    public int PredictedHomeScore { get; set; }

    public int PredictedAwayScore { get; set; }

    public int RealHomeScore { get; set; }

    public int RealAwayScore { get; set; }

    public string MatchDateText { get; set; } = string.Empty;

    public string MatchTimeText { get; set; } = string.Empty;

    public int PointsEarned { get; set; }

    public bool ExactScoreHit { get; set; }

    public bool WinnerHit { get; set; }
}
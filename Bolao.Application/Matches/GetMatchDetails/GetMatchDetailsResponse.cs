namespace Bolao.Application.Matches.GetMatchDetails;

public sealed class GetMatchDetailsResponse
{
    public Guid MatchId { get; set; }

    public Guid BolaoId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string GroupName { get; set; } = string.Empty;

    public string RoundName { get; set; } = string.Empty;

    public DateTime? MatchDate { get; set; }

    public string MatchDateText { get; set; } = string.Empty;

    public string MatchTimeText { get; set; } = string.Empty;

    public string StadiumName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public TeamDetailsResponse HomeTeam { get; set; } = new();

    public TeamDetailsResponse AwayTeam { get; set; } = new();

    public int? HomeScore { get; set; }

    public int? AwayScore { get; set; }

    public bool CanPredict { get; set; }

    public DateTime? PredictionDeadline { get; set; }

    public MyPredictionResponse? MyPrediction { get; set; }
}

public sealed class TeamDetailsResponse
{
    public int ExternalId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Acronym { get; set; } = string.Empty;

    public string LogoUrl { get; set; } = string.Empty;
}

public sealed class MyPredictionResponse
{
    public Guid PredictionId { get; set; }

    public int HomeScore { get; set; }

    public int AwayScore { get; set; }

    public int PointsEarned { get; set; }

    public bool ExactScoreHit { get; set; }

    public bool WinnerHit { get; set; }

    public bool HomeGoalsHit { get; set; }

    public bool AwayGoalsHit { get; set; }
}
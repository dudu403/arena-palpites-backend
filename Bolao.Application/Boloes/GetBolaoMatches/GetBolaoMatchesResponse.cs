namespace Bolao.Application.Boloes.GetBolaoMatches;

public sealed class GetBolaoMatchesResponse
{
    public Guid BolaoId { get; set; }

    public string BolaoName { get; set; } = string.Empty;

    public string Championship { get; set; } = string.Empty;

    public int ChampionshipExternalId { get; set; }

    public List<BolaoMatchesRoundResponse> Rounds { get; set; } = [];
}

public sealed class BolaoMatchesRoundResponse
{
    public int? RoundNumber { get; set; }

    public string RoundName { get; set; } = string.Empty;

    public List<BolaoMatchResponse> Matches { get; set; } = [];
}

public sealed class BolaoMatchResponse
{
    public Guid MatchId { get; set; }

    public string GroupName { get; set; } = string.Empty;

    public string RoundName { get; set; } = string.Empty;

    public DateTime? MatchDate { get; set; }

    public string MatchDateText { get; set; } = string.Empty;

    public string MatchTimeText { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public bool IsFinished { get; set; }

    public bool CanPredict { get; set; }

    public string ResultText { get; set; } = string.Empty;

    public int? HomeScore { get; set; }

    public int? AwayScore { get; set; }

    public bool HasPrediction { get; set; }

    public int? MyPredictionHomeScore { get; set; }

    public int? MyPredictionAwayScore { get; set; }

    public int PointsEarned { get; set; }

    public bool ExactScoreHit { get; set; }

    public bool WinnerHit { get; set; }

    public BolaoMatchTeamResponse HomeTeam { get; set; } = new();

    public BolaoMatchTeamResponse AwayTeam { get; set; } = new();
}

public sealed class BolaoMatchTeamResponse
{
    public int ExternalId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Acronym { get; set; } = string.Empty;

    public string LogoUrl { get; set; } = string.Empty;
}
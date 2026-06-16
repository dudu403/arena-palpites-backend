namespace Bolao.Application.Boloes.GetBolaoCurrentRoundMatches;

public sealed class GetBolaoCurrentRoundMatchesResponse
{
    public Guid BolaoId { get; set; }

    public string BolaoName { get; set; } = string.Empty;

    public string Championship { get; set; } = string.Empty;

    public int ChampionshipExternalId { get; set; }

    public BolaoCurrentRoundResponse? CurrentRound { get; set; }
}

public sealed class BolaoCurrentRoundResponse
{
    public int? RoundNumber { get; set; }

    public string RoundName { get; set; } = string.Empty;

    public List<BolaoCurrentRoundMatchResponse> Matches { get; set; } = [];
}

public sealed class BolaoCurrentRoundMatchResponse
{
    public Guid MatchId { get; set; }

    public string GroupName { get; set; } = string.Empty;

    public string RoundName { get; set; } = string.Empty;

    public DateTime? MatchDate { get; set; }

    public string MatchDateText { get; set; } = string.Empty;

    public string MatchTimeText { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public bool IsFinished { get; set; }

    public string ResultText { get; set; } = string.Empty;

    public int? HomeScore { get; set; }

    public int? AwayScore { get; set; }

    public bool HasPrediction { get; set; }

    public int? MyPredictionHomeScore { get; set; }

    public int? MyPredictionAwayScore { get; set; }

    public int PointsEarned { get; set; }

    public bool ExactScoreHit { get; set; }

    public bool WinnerHit { get; set; }

    public BolaoCurrentRoundTeamResponse HomeTeam { get; set; } = new();

    public BolaoCurrentRoundTeamResponse AwayTeam { get; set; } = new();
}

public sealed class BolaoCurrentRoundTeamResponse
{
    public int ExternalId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Acronym { get; set; } = string.Empty;

    public string LogoUrl { get; set; } = string.Empty;
}
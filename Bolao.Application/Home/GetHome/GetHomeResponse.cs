namespace Bolao.Application.Home.GetHome;

public sealed class GetHomeResponse
{
    public string UserName { get; set; } = string.Empty;

    public int TotalPools { get; set; }

    public int PendingPredictions { get; set; }

    public int TotalPredictions { get; set; }

    public int TotalPoints { get; set; }

    public List<HomePoolResponse> Pools { get; set; } = [];

    public List<HomeMatchResponse> LastChanceMatches { get; set; } = [];

    public List<HomeMatchResponse> PendingMatches { get; set; } = [];

    public List<HomeMatchResponse> UpcomingMatches { get; set; } = [];
}

public sealed class HomePoolResponse
{
    public Guid BolaoId { get; set; }

    public string BolaoName { get; set; } = string.Empty;

    public int MembersCount { get; set; }

    public int Position { get; set; }

    public int Points { get; set; }

    public int PredictionsCount { get; set; }
}

public sealed class HomeMatchResponse
{
    public Guid MatchId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string GroupName { get; set; } = string.Empty;

    public string RoundName { get; set; } = string.Empty;

    public DateTime? MatchDate { get; set; }

    public string MatchDateText { get; set; } = string.Empty;

    public string MatchTimeText { get; set; } = string.Empty;

    public bool HasPrediction { get; set; }

    public bool LastChance { get; set; }

    public HomeTeamResponse HomeTeam { get; set; } = new();

    public HomeTeamResponse AwayTeam { get; set; } = new();
}

public sealed class HomeTeamResponse
{
    public int ExternalId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Acronym { get; set; } = string.Empty;

    public string LogoUrl { get; set; } = string.Empty;
}
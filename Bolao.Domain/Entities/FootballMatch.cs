namespace Bolao.Domain.Entities;

public class FootballMatch
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public int ExternalId { get; set; }

    public int ChampionshipExternalId { get; set; }

    public int? PhaseExternalId { get; set; }

    public string GroupName { get; set; } = string.Empty;

    public string GroupSlug { get; set; } = string.Empty;

    public string RoundName { get; set; } = string.Empty;

    public string RoundSlug { get; set; } = string.Empty;

    public int? RoundNumber { get; set; }

    public string ScoreText { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public int HomeTeamExternalId { get; set; }

    public int AwayTeamExternalId { get; set; }

    public int? HomeScore { get; set; }

    public int? AwayScore { get; set; }

    public bool HasPenaltyShootout { get; set; }

    public DateTime? MatchDate { get; set; }

    public string MatchDateText { get; set; } = string.Empty;

    public string MatchTimeText { get; set; } = string.Empty;

    public int? StadiumExternalId { get; set; }

    public string StadiumName { get; set; } = string.Empty;

    public bool PointsCalculated { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
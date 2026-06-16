namespace Bolao.Domain.Entities;

public class FootballGroupStanding
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public int ChampionshipExternalId { get; set; }

    public int? PhaseExternalId { get; set; }

    public string GroupName { get; set; } = string.Empty;

    public string GroupSlug { get; set; } = string.Empty;

    public int TeamExternalId { get; set; }

    public int Position { get; set; }

    public int Points { get; set; }

    public int Games { get; set; }

    public int Wins { get; set; }

    public int Draws { get; set; }

    public int Losses { get; set; }

    public int GoalsFor { get; set; }

    public int GoalsAgainst { get; set; }

    public int GoalDifference { get; set; }

    public decimal Performance { get; set; }

    public int PositionVariation { get; set; }

    public string? QualificationZone { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
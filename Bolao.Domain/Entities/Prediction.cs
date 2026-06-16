namespace Bolao.Domain.Entities;

public class Prediction
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid BolaoId { get; set; }

    public Guid UserId { get; set; }

    public Guid FootballMatchId { get; set; }

    public int HomeScore { get; set; }

    public int AwayScore { get; set; }

    public int PointsEarned { get; set; }

    public bool ExactScoreHit { get; set; }

    public bool WinnerHit { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public Bolao Bolao { get; set; } = null!;

    public User User { get; set; } = null!;

    public FootballMatch FootballMatch { get; set; } = null!;
}
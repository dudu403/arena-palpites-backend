namespace Bolao.Application.Boloes.GetBolaoDashboard;

public sealed class GetBolaoDashboardResponse
{
    public Guid BolaoId { get; set; }

    public string BolaoName { get; set; } = string.Empty;

    public int TotalMembers { get; set; }

    public int TotalPredictions { get; set; }

    public int MatchesFinished { get; set; }

    public int MatchesRemaining { get; set; }

    public string? LeaderName { get; set; }

    public string? LeaderPhotoUrl { get; set; }

    public int LeaderPoints { get; set; }

    public int MyPosition { get; set; }

    public int MyPoints { get; set; }

    public Guid? NextMatchId { get; set; }

    public string? NextMatchTitle { get; set; }

    public DateTime? NextMatchDate { get; set; }

    public string? NextMatchDateText { get; set; }

    public string? NextMatchTimeText { get; set; }
}
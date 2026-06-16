namespace Bolao.Application.Rankings.GetRankingDetails;

public sealed class GetRankingDetailsResponse
{
    public int Position { get; set; }

    public Guid UserId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string? PhotoUrl { get; set; }

    public int TotalPoints { get; set; }

    public int PredictionsCount { get; set; }

    public int ExactScores { get; set; }

    public int WinnerHits { get; set; }

    public int GoalHits { get; set; }
}
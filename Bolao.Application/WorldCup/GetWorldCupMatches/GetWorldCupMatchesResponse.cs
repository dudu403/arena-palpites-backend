namespace Bolao.Application.WorldCup.GetWorldCupMatches;

public sealed class GetWorldCupMatchesResponse
{
    public Guid Id { get; set; }
    public int ExternalId { get; set; }

    public string GroupName { get; set; } = string.Empty;
    public string GroupSlug { get; set; } = string.Empty;

    public string RoundName { get; set; } = string.Empty;
    public int? RoundNumber { get; set; }

    public string HomeTeamName { get; set; } = string.Empty;
    public string HomeTeamAcronym { get; set; } = string.Empty;
    public string HomeTeamLogoUrl { get; set; } = string.Empty;

    public string AwayTeamName { get; set; } = string.Empty;
    public string AwayTeamAcronym { get; set; } = string.Empty;
    public string AwayTeamLogoUrl { get; set; } = string.Empty;

    public DateTime? MatchDate { get; set; }
    public string MatchDateText { get; set; } = string.Empty;
    public string MatchTimeText { get; set; } = string.Empty;

    public string StadiumName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }
}
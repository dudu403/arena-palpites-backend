namespace Bolao.Application.WorldCup.GetWorldCupMatches;

public sealed class GetWorldCupMatchesQuery
{
    public int? RoundNumber { get; set; }
    public string? GroupSlug { get; set; }
}
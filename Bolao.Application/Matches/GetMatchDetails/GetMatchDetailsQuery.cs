namespace Bolao.Application.Matches.GetMatchDetails;

public sealed class GetMatchDetailsQuery
{
    public Guid MatchId { get; set; }

    public Guid BolaoId { get; set; }
}
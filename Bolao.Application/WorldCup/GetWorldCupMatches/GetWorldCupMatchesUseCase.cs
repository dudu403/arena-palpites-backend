using Bolao.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bolao.Application.WorldCup.GetWorldCupMatches;

public sealed class GetWorldCupMatchesUseCase
{
    private const int WorldCupChampionshipId = 72;

    private readonly IApplicationDbContext _context;

    public GetWorldCupMatchesUseCase(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<GetWorldCupMatchesResponse>> ExecuteAsync(
        GetWorldCupMatchesQuery query,
        CancellationToken cancellationToken)
    {
        var matchesQuery = _context.FootballMatches
            .AsNoTracking()
            .Where(x => x.ChampionshipExternalId == WorldCupChampionshipId);

        if (query.RoundNumber.HasValue)
        {
            matchesQuery = matchesQuery
                .Where(x => x.RoundNumber == query.RoundNumber.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.GroupSlug))
        {
            var groupSlug = query.GroupSlug.Trim().ToLower();

            matchesQuery = matchesQuery
                .Where(x => x.GroupSlug == groupSlug);
        }

        var teamsQuery = _context.FootballTeams.AsNoTracking();

        var result = await matchesQuery
            .OrderBy(x => x.MatchDate)
            .ThenBy(x => x.GroupSlug)
            .ThenBy(x => x.RoundNumber)
            .Select(match => new GetWorldCupMatchesResponse
            {
                Id = match.Id,
                ExternalId = match.ExternalId,

                GroupName = match.GroupName,
                GroupSlug = match.GroupSlug,

                RoundName = match.RoundName,
                RoundNumber = match.RoundNumber,

                HomeTeamName = teamsQuery
                    .Where(team => team.ExternalId == match.HomeTeamExternalId)
                    .Select(team => team.Name)
                    .FirstOrDefault() ?? string.Empty,

                HomeTeamAcronym = teamsQuery
                    .Where(team => team.ExternalId == match.HomeTeamExternalId)
                    .Select(team => team.Acronym)
                    .FirstOrDefault() ?? string.Empty,

                HomeTeamLogoUrl = teamsQuery
                    .Where(team => team.ExternalId == match.HomeTeamExternalId)
                    .Select(team => team.LogoUrl)
                    .FirstOrDefault() ?? string.Empty,

                AwayTeamName = teamsQuery
                    .Where(team => team.ExternalId == match.AwayTeamExternalId)
                    .Select(team => team.Name)
                    .FirstOrDefault() ?? string.Empty,

                AwayTeamAcronym = teamsQuery
                    .Where(team => team.ExternalId == match.AwayTeamExternalId)
                    .Select(team => team.Acronym)
                    .FirstOrDefault() ?? string.Empty,

                AwayTeamLogoUrl = teamsQuery
                    .Where(team => team.ExternalId == match.AwayTeamExternalId)
                    .Select(team => team.LogoUrl)
                    .FirstOrDefault() ?? string.Empty,

                MatchDate = match.MatchDate,
                MatchDateText = match.MatchDateText,
                MatchTimeText = match.MatchTimeText,

                StadiumName = match.StadiumName,
                Status = match.Status,

                HomeScore = match.HomeScore,
                AwayScore = match.AwayScore
            })
            .ToListAsync(cancellationToken);

        return result;
    }
}
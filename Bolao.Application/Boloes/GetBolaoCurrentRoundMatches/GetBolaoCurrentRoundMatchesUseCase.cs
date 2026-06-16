using Bolao.Application.Common.Exceptions;
using Bolao.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bolao.Application.Boloes.GetBolaoCurrentRoundMatches;

public sealed class GetBolaoCurrentRoundMatchesUseCase
{
    private const int PredictionDeadlineMinutes = 15;

    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetBolaoCurrentRoundMatchesUseCase(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<GetBolaoCurrentRoundMatchesResponse> ExecuteAsync(
        GetBolaoCurrentRoundMatchesQuery query,
        CancellationToken cancellationToken)
    {
        var firebaseUid = _currentUserService.FirebaseUid;

        if (string.IsNullOrWhiteSpace(firebaseUid))
            throw new UnauthorizedException("Usuário não autenticado.");

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.FirebaseUid == firebaseUid, cancellationToken);

        if (user is null)
            throw new UnauthorizedException("Usuário não encontrado.");

        var bolao = await _context.Boloes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == query.BolaoId, cancellationToken);

        if (bolao is null)
            throw new NotFoundException("Bolão não encontrado.");

        var isMember = await _context.BolaoMembers
            .AsNoTracking()
            .AnyAsync(
                x => x.BolaoId == bolao.Id && x.UserId == user.Id,
                cancellationToken);

        if (!isMember)
            throw new ForbiddenException("Você não participa deste bolão.");

        var deadlineLimit = DateTime.UtcNow.AddMinutes(PredictionDeadlineMinutes);

        var matches = await _context.FootballMatches
            .AsNoTracking()
            .Where(x =>
                x.ChampionshipExternalId == bolao.ChampionshipExternalId &&
                x.MatchDate != null &&
                x.MatchDate > deadlineLimit &&
                x.Status != "finalizado")
            .OrderBy(x => x.RoundNumber)
            .ThenBy(x => x.MatchDate)
            .ToListAsync(cancellationToken);

        if (matches.Count == 0)
        {
            return new GetBolaoCurrentRoundMatchesResponse
            {
                BolaoId = bolao.Id,
                BolaoName = bolao.Name,
                Championship = bolao.Championship,
                ChampionshipExternalId = bolao.ChampionshipExternalId,
                CurrentRound = null
            };
        }

        var firstAvailableRoundNumber = matches
            .OrderBy(x => x.RoundNumber ?? int.MaxValue)
            .ThenBy(x => x.MatchDate)
            .Select(x => x.RoundNumber)
            .FirstOrDefault();

        var selectedRoundMatches = matches
            .Where(x => x.RoundNumber == firstAvailableRoundNumber)
            .OrderBy(x => x.MatchDate)
            .ToList();

        var roundName = selectedRoundMatches
            .Select(x => x.RoundName)
            .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? "Rodada";

        var teamIds = selectedRoundMatches
            .SelectMany(x => new[] { x.HomeTeamExternalId, x.AwayTeamExternalId })
            .Where(x => x > 0)
            .Distinct()
            .ToList();

        var teams = await _context.FootballTeams
            .AsNoTracking()
            .Where(x => teamIds.Contains(x.ExternalId))
            .ToDictionaryAsync(x => x.ExternalId, cancellationToken);

        var matchIds = selectedRoundMatches
            .Select(x => x.Id)
            .ToList();

        var predictions = await _context.Predictions
            .AsNoTracking()
            .Where(x =>
                x.BolaoId == bolao.Id &&
                x.UserId == user.Id &&
                matchIds.Contains(x.FootballMatchId))
            .ToDictionaryAsync(x => x.FootballMatchId, cancellationToken);

        var currentRound = new BolaoCurrentRoundResponse
        {
            RoundNumber = firstAvailableRoundNumber,
            RoundName = roundName,
            Matches = selectedRoundMatches.Select(match =>
            {
                teams.TryGetValue(match.HomeTeamExternalId, out var homeTeam);
                teams.TryGetValue(match.AwayTeamExternalId, out var awayTeam);
                predictions.TryGetValue(match.Id, out var prediction);

                var status = match.Status ?? string.Empty;
                var isFinished = IsFinishedStatus(status);

                return new BolaoCurrentRoundMatchResponse
                {
                    MatchId = match.Id,
                    GroupName = match.GroupName,
                    RoundName = match.RoundName,
                    MatchDate = match.MatchDate,
                    MatchDateText = match.MatchDateText,
                    MatchTimeText = match.MatchTimeText,
                    Status = status,
                    IsFinished = isFinished,
                    ResultText = isFinished ? match.ScoreText : string.Empty,
                    HomeScore = match.HomeScore,
                    AwayScore = match.AwayScore,
                    HasPrediction = prediction is not null,
                    MyPredictionHomeScore = prediction?.HomeScore,
                    MyPredictionAwayScore = prediction?.AwayScore,
                    PointsEarned = prediction?.PointsEarned ?? 0,
                    ExactScoreHit = prediction?.ExactScoreHit ?? false,
                    WinnerHit = prediction?.WinnerHit ?? false,
                    HomeTeam = new BolaoCurrentRoundTeamResponse
                    {
                        ExternalId = match.HomeTeamExternalId,
                        Name = homeTeam?.Name ?? string.Empty,
                        Acronym = homeTeam?.Acronym ?? string.Empty,
                        LogoUrl = homeTeam?.LogoUrl ?? string.Empty
                    },
                    AwayTeam = new BolaoCurrentRoundTeamResponse
                    {
                        ExternalId = match.AwayTeamExternalId,
                        Name = awayTeam?.Name ?? string.Empty,
                        Acronym = awayTeam?.Acronym ?? string.Empty,
                        LogoUrl = awayTeam?.LogoUrl ?? string.Empty
                    }
                };
            }).ToList()
        };

        return new GetBolaoCurrentRoundMatchesResponse
        {
            BolaoId = bolao.Id,
            BolaoName = bolao.Name,
            Championship = bolao.Championship,
            ChampionshipExternalId = bolao.ChampionshipExternalId,
            CurrentRound = currentRound
        };
    }

    private static bool IsFinishedStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return false;

        var normalized = status.Trim().ToLowerInvariant();

        return normalized is "finalizado" or "finished" or "encerrado";
    }
}
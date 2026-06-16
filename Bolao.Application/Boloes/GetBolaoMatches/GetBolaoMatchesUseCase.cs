using Bolao.Application.Common.Exceptions;
using Bolao.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bolao.Application.Boloes.GetBolaoMatches;

public sealed class GetBolaoMatchesUseCase
{
    private const int PredictionDeadlineMinutes = 15;

    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetBolaoMatchesUseCase(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<GetBolaoMatchesResponse> ExecuteAsync(
        GetBolaoMatchesQuery query,
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

        var matches = await _context.FootballMatches
            .AsNoTracking()
            .Where(x => x.ChampionshipExternalId == bolao.ChampionshipExternalId)
            .OrderBy(x => x.RoundNumber)
            .ThenBy(x => x.MatchDate)
            .ToListAsync(cancellationToken);

        var teamIds = matches
            .SelectMany(x => new[] { x.HomeTeamExternalId, x.AwayTeamExternalId })
            .Where(x => x > 0)
            .Distinct()
            .ToList();

        var teams = await _context.FootballTeams
            .AsNoTracking()
            .Where(x => teamIds.Contains(x.ExternalId))
            .ToDictionaryAsync(x => x.ExternalId, cancellationToken);

        var matchIds = matches
            .Select(x => x.Id)
            .ToList();

        var predictions = await _context.Predictions
            .AsNoTracking()
            .Where(x =>
                x.BolaoId == bolao.Id &&
                x.UserId == user.Id &&
                matchIds.Contains(x.FootballMatchId))
            .ToDictionaryAsync(x => x.FootballMatchId, cancellationToken);

        var utcNow = DateTime.UtcNow;

        var rounds = matches
            .GroupBy(x => new
            {
                x.RoundNumber,
                RoundName = string.IsNullOrWhiteSpace(x.RoundName)
                    ? "Rodada"
                    : x.RoundName
            })
            .OrderBy(x => x.Key.RoundNumber ?? int.MaxValue)
            .ThenBy(x => x.Key.RoundName)
            .Select(round => new BolaoMatchesRoundResponse
            {
                RoundNumber = round.Key.RoundNumber,
                RoundName = round.Key.RoundName,
                Matches = round.Select(match =>
                {
                    teams.TryGetValue(match.HomeTeamExternalId, out var homeTeam);
                    teams.TryGetValue(match.AwayTeamExternalId, out var awayTeam);
                    predictions.TryGetValue(match.Id, out var prediction);

                    var status = match.Status ?? string.Empty;
                    var isFinished = IsFinishedStatus(status);
                    var canPredict = CanPredict(match.MatchDate, status, utcNow);

                    return new BolaoMatchResponse
                    {
                        MatchId = match.Id,
                        GroupName = match.GroupName,
                        RoundName = match.RoundName,
                        MatchDate = match.MatchDate,
                        MatchDateText = match.MatchDateText,
                        MatchTimeText = match.MatchTimeText,
                        Status = status,
                        IsFinished = isFinished,
                        CanPredict = canPredict,
                        ResultText = isFinished ? match.ScoreText : string.Empty,
                        HomeScore = match.HomeScore,
                        AwayScore = match.AwayScore,
                        HasPrediction = prediction is not null,
                        MyPredictionHomeScore = prediction?.HomeScore,
                        MyPredictionAwayScore = prediction?.AwayScore,
                        PointsEarned = prediction?.PointsEarned ?? 0,
                        ExactScoreHit = prediction?.ExactScoreHit ?? false,
                        WinnerHit = prediction?.WinnerHit ?? false,
                        HomeTeam = new BolaoMatchTeamResponse
                        {
                            ExternalId = match.HomeTeamExternalId,
                            Name = homeTeam?.Name ?? string.Empty,
                            Acronym = homeTeam?.Acronym ?? string.Empty,
                            LogoUrl = homeTeam?.LogoUrl ?? string.Empty
                        },
                        AwayTeam = new BolaoMatchTeamResponse
                        {
                            ExternalId = match.AwayTeamExternalId,
                            Name = awayTeam?.Name ?? string.Empty,
                            Acronym = awayTeam?.Acronym ?? string.Empty,
                            LogoUrl = awayTeam?.LogoUrl ?? string.Empty
                        }
                    };
                }).ToList()
            })
            .ToList();

        return new GetBolaoMatchesResponse
        {
            BolaoId = bolao.Id,
            BolaoName = bolao.Name,
            Championship = bolao.Championship,
            ChampionshipExternalId = bolao.ChampionshipExternalId,
            Rounds = rounds
        };
    }

    private static bool CanPredict(
        DateTime? matchDate,
        string status,
        DateTime utcNow)
    {
        if (matchDate is null)
            return false;

        if (IsBlockedStatus(status))
            return false;

        var predictionDeadline = matchDate.Value.AddMinutes(-PredictionDeadlineMinutes);

        return utcNow < predictionDeadline;
    }

    private static bool IsFinishedStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return false;

        var normalized = status.Trim().ToLowerInvariant();

        return normalized is "finalizado" or "finished" or "encerrado";
    }

    private static bool IsBlockedStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return false;

        var normalized = status.Trim().ToLowerInvariant();

        return normalized is
            "andamento" or
            "ao vivo" or
            "live" or
            "em andamento" or
            "intervalo" or
            "finalizado" or
            "finished" or
            "encerrado" or
            "adiado" or
            "cancelado";
    }
}
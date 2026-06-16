using Bolao.Application.Common.Exceptions;
using Bolao.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bolao.Application.Boloes.GetMemberFinishedPredictions;

public sealed class GetMemberFinishedPredictionsUseCase
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetMemberFinishedPredictionsUseCase(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<GetMemberFinishedPredictionsResponse> ExecuteAsync(
        GetMemberFinishedPredictionsQuery query,
        CancellationToken cancellationToken)
    {
        var firebaseUid = _currentUserService.FirebaseUid;

        if (string.IsNullOrWhiteSpace(firebaseUid))
            throw new UnauthorizedException("Usuário não autenticado.");

        var currentUser = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.FirebaseUid == firebaseUid, cancellationToken);

        if (currentUser is null)
            throw new UnauthorizedException("Usuário não encontrado.");

        var requesterIsMember = await _context.BolaoMembers
            .AsNoTracking()
            .AnyAsync(
                x => x.BolaoId == query.BolaoId &&
                     x.UserId == currentUser.Id,
                cancellationToken);

        if (!requesterIsMember)
            throw new ForbiddenException("Você não participa deste bolão.");

        var targetMember = await _context.BolaoMembers
            .AsNoTracking()
            .Where(x => x.BolaoId == query.BolaoId && x.UserId == query.UserId)
            .Join(
                _context.Users.AsNoTracking(),
                member => member.UserId,
                user => user.Id,
                (member, user) => new
                {
                    user.Id,
                    user.Name,
                    user.PhotoUrl
                })
            .FirstOrDefaultAsync(cancellationToken);

        if (targetMember is null)
            throw new NotFoundException("Participante não encontrado neste bolão.");

        var data = await _context.Predictions
            .AsNoTracking()
            .Where(prediction =>
                prediction.BolaoId == query.BolaoId &&
                prediction.UserId == query.UserId)
            .Join(
                _context.FootballMatches.AsNoTracking(),
                prediction => prediction.FootballMatchId,
                match => match.Id,
                (prediction, match) => new
                {
                    Prediction = prediction,
                    Match = match
                })
            .Where(x =>
                x.Match.Status == "finalizado" &&
                x.Match.HomeScore.HasValue &&
                x.Match.AwayScore.HasValue)
            .OrderByDescending(x => x.Match.MatchDate)
            .ToListAsync(cancellationToken);

        var teamIds = data
            .SelectMany(x => new[]
            {
                x.Match.HomeTeamExternalId,
                x.Match.AwayTeamExternalId
            })
            .Where(x => x > 0)
            .Distinct()
            .ToList();

        var teams = await _context.FootballTeams
            .AsNoTracking()
            .Where(x => teamIds.Contains(x.ExternalId))
            .ToDictionaryAsync(x => x.ExternalId, cancellationToken);

        var matches = data
            .Select(x =>
            {
                teams.TryGetValue(x.Match.HomeTeamExternalId, out var homeTeam);
                teams.TryGetValue(x.Match.AwayTeamExternalId, out var awayTeam);

                return new MemberFinishedPredictionMatchResponse
                {
                    PredictionId = x.Prediction.Id,
                    FootballMatchId = x.Match.Id,

                    HomeTeamName = homeTeam?.Name ?? string.Empty,
                    AwayTeamName = awayTeam?.Name ?? string.Empty,

                    PredictedHomeScore = x.Prediction.HomeScore,
                    PredictedAwayScore = x.Prediction.AwayScore,

                    RealHomeScore = x.Match.HomeScore!.Value,
                    RealAwayScore = x.Match.AwayScore!.Value,

                    MatchDateText = x.Match.MatchDateText,
                    MatchTimeText = x.Match.MatchTimeText,

                    PointsEarned = x.Prediction.PointsEarned,
                    ExactScoreHit = x.Prediction.ExactScoreHit,
                    WinnerHit = x.Prediction.WinnerHit
                };
            })
            .ToList();

        return new GetMemberFinishedPredictionsResponse
        {
            UserId = targetMember.Id,
            UserName = targetMember.Name,
            PhotoUrl = targetMember.PhotoUrl,
            TotalPoints = matches.Sum(x => x.PointsEarned),
            PredictionsCount = matches.Count,
            Matches = matches
        };
    }
}
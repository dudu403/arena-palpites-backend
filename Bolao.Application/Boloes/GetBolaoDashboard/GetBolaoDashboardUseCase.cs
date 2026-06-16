using Bolao.Application.Common.Caching;
using Bolao.Application.Common.Exceptions;
using Bolao.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bolao.Application.Boloes.GetBolaoDashboard;

public sealed class GetBolaoDashboardUseCase
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(30);

    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;

    public GetBolaoDashboardUseCase(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ICacheService cacheService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
    }

    public async Task<GetBolaoDashboardResponse> ExecuteAsync(
        GetBolaoDashboardQuery query,
        CancellationToken cancellationToken)
    {
        var firebaseUid = _currentUserService.FirebaseUid;

        if (string.IsNullOrWhiteSpace(firebaseUid))
            throw new UnauthorizedException("Usuário não autenticado.");

        var cacheKey = CacheKeys.BolaoDashboard(
            query.BolaoId,
            firebaseUid);

        var cachedResponse =
            _cacheService.Get<GetBolaoDashboardResponse>(cacheKey);

        if (cachedResponse is not null)
            return cachedResponse;

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.FirebaseUid == firebaseUid,
                cancellationToken);

        if (user is null)
            throw new UnauthorizedException("Usuário não encontrado.");

        var bolao = await _context.Boloes
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == query.BolaoId,
                cancellationToken);

        if (bolao is null)
            throw new NotFoundException("Bolão não encontrado.");

        var isMember = await _context.BolaoMembers
            .AsNoTracking()
            .AnyAsync(
                x => x.BolaoId == query.BolaoId &&
                     x.UserId == user.Id,
                cancellationToken);

        if (!isMember)
            throw new ForbiddenException("Você não participa deste bolão.");

        var members = await _context.BolaoMembers
            .AsNoTracking()
            .Where(x => x.BolaoId == query.BolaoId)
            .Select(x => x.UserId)
            .ToListAsync(cancellationToken);

        var totalMembers = members.Count;

        var predictionsQuery = _context.Predictions
            .AsNoTracking()
            .Where(x => x.BolaoId == query.BolaoId);

        var totalPredictions = await predictionsQuery
            .CountAsync(cancellationToken);

        var matchesFinished = await _context.FootballMatches
            .AsNoTracking()
            .CountAsync(
                x => x.Status == "finalizado",
                cancellationToken);

        var matchesRemaining = await _context.FootballMatches
            .AsNoTracking()
            .CountAsync(
                x => x.Status != "finalizado",
                cancellationToken);

        var rankingData = await _context.BolaoMembers
            .AsNoTracking()
            .Where(member => member.BolaoId == query.BolaoId)
            .Join(
                _context.Users.AsNoTracking(),
                member => member.UserId,
                rankingUser => rankingUser.Id,
                (member, rankingUser) => new
                {
                    member.UserId,
                    rankingUser.Name,
                    rankingUser.PhotoUrl
                })
            .GroupJoin(
                _context.Predictions
                    .AsNoTracking()
                    .Where(prediction => prediction.BolaoId == query.BolaoId),
                member => member.UserId,
                prediction => prediction.UserId,
                (member, predictions) => new
                {
                    member.UserId,
                    UserName = member.Name,
                    member.PhotoUrl,
                    TotalPoints = predictions.Sum(x => x.PointsEarned),
                    PredictionsCount = predictions.Count(),
                    ExactScores = predictions.Count(x => x.ExactScoreHit)
                })
            .OrderByDescending(x => x.TotalPoints)
            .ThenByDescending(x => x.ExactScores)
            .ThenByDescending(x => x.PredictionsCount)
            .ThenBy(x => x.UserName)
            .ToListAsync(cancellationToken);

        var indexedRanking = rankingData
            .Select((item, index) => new
            {
                Position = index + 1,
                item.UserId,
                item.UserName,
                item.PhotoUrl,
                item.TotalPoints
            })
            .ToList();

        var leader = indexedRanking.FirstOrDefault();
        var me = indexedRanking.FirstOrDefault(x => x.UserId == user.Id);

        var nextMatch = await _context.FootballMatches
            .AsNoTracking()
            .Where(x =>
                x.MatchDate != null &&
                x.MatchDate > DateTime.UtcNow &&
                x.Status != "finalizado")
            .OrderBy(x => x.MatchDate)
            .Select(x => new
            {
                x.Id,
                x.ScoreText,
                x.MatchDate,
                x.MatchDateText,
                x.MatchTimeText
            })
            .FirstOrDefaultAsync(cancellationToken);

        var response = new GetBolaoDashboardResponse
        {
            BolaoId = bolao.Id,
            BolaoName = bolao.Name,

            TotalMembers = totalMembers,
            TotalPredictions = totalPredictions,

            MatchesFinished = matchesFinished,
            MatchesRemaining = matchesRemaining,

            LeaderName = leader?.UserName,
            LeaderPhotoUrl = leader?.PhotoUrl,
            LeaderPoints = leader?.TotalPoints ?? 0,

            MyPosition = me?.Position ?? 0,
            MyPoints = me?.TotalPoints ?? 0,

            NextMatchId = nextMatch?.Id,
            NextMatchTitle = nextMatch?.ScoreText,
            NextMatchDate = nextMatch?.MatchDate,
            NextMatchDateText = nextMatch?.MatchDateText,
            NextMatchTimeText = nextMatch?.MatchTimeText
        };

        _cacheService.Set(
            cacheKey,
            response,
            CacheDuration);

        return response;
    }
}
using Bolao.Application.Common.Caching;
using Bolao.Application.Common.Exceptions;
using Bolao.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bolao.Application.Boloes.GetBolaoRanking;

public sealed class GetBolaoRankingUseCase
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(30);

    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;

    public GetBolaoRankingUseCase(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ICacheService cacheService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
    }

    public async Task<List<GetBolaoRankingResponse>> ExecuteAsync(
        GetBolaoRankingQuery query,
        CancellationToken cancellationToken)
    {
        var firebaseUid = _currentUserService.FirebaseUid;

        if (string.IsNullOrWhiteSpace(firebaseUid))
            throw new UnauthorizedException("Usuário não autenticado.");

        var currentUser = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.FirebaseUid == firebaseUid,
                cancellationToken);

        if (currentUser is null)
            throw new UnauthorizedException("Usuário não encontrado.");

        var isMember = await _context.BolaoMembers
            .AsNoTracking()
            .AnyAsync(
                x => x.BolaoId == query.BolaoId &&
                     x.UserId == currentUser.Id,
                cancellationToken);

        if (!isMember)
            throw new ForbiddenException("Você não participa deste bolão.");

        var cacheKey = CacheKeys.BolaoRanking(query.BolaoId);

        var cachedRanking =
            _cacheService.Get<List<GetBolaoRankingResponse>>(cacheKey);

        if (cachedRanking is not null)
            return cachedRanking;

        var ranking = await _context.BolaoMembers
            .AsNoTracking()
            .Where(member => member.BolaoId == query.BolaoId)
            .Join(
                _context.Users.AsNoTracking(),
                member => member.UserId,
                user => user.Id,
                (member, user) => new
                {
                    member.UserId,
                    user.Name,
                    user.PhotoUrl
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

                    ExactScores = predictions.Count(x => x.ExactScoreHit),
                    WinnerHits = predictions.Count(x => x.WinnerHit)
                })
            .OrderByDescending(x => x.TotalPoints)
            .ThenByDescending(x => x.ExactScores)
            .ThenByDescending(x => x.WinnerHits)
            .ThenBy(x => x.UserName)
            .ToListAsync(cancellationToken);

        var response = ranking
            .Select((item, index) => new GetBolaoRankingResponse
            {
                Position = index + 1,
                UserId = item.UserId,
                UserName = item.UserName,
                PhotoUrl = item.PhotoUrl,
                TotalPoints = item.TotalPoints,
                PredictionsCount = item.PredictionsCount,
                ExactScores = item.ExactScores,
                WinnerHits = item.WinnerHits
            })
            .ToList();

        _cacheService.Set(cacheKey, response, CacheDuration);

        return response;
    }
}
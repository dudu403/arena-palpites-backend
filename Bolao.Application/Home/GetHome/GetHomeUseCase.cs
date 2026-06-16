using Bolao.Application.Common.Caching;
using Bolao.Application.Common.Exceptions;
using Bolao.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bolao.Application.Home.GetHome;

public sealed class GetHomeUseCase
{
    private const int PredictionDeadlineMinutes = 15;
    private const int LastChanceWindowMinutes = 120;
    private const int MaxHomePools = 3;
    private const int MaxTodayMatches = 20;
    private const int MaxSectionItems = 5;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(15);

    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;

    public GetHomeUseCase(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ICacheService cacheService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
    }

    public async Task<GetHomeResponse> ExecuteAsync(
        GetHomeQuery query,
        CancellationToken cancellationToken)
    {
        var firebaseUid = _currentUserService.FirebaseUid;

        if (string.IsNullOrWhiteSpace(firebaseUid))
            throw new UnauthorizedException("Usuário não autenticado.");

        var cacheKey = CacheKeys.Home(firebaseUid);
        var cachedResponse = _cacheService.Get<GetHomeResponse>(cacheKey);

        if (cachedResponse is not null)
            return cachedResponse;

        var brazilTimeZone = GetBrazilTimeZone();

        var nowUtc = DateTime.UtcNow;
        var nowBrazil = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, brazilTimeZone);

        var todayBrazilStart = nowBrazil.Date;
        var tomorrowBrazilStart = todayBrazilStart.AddDays(1);

        var todayUtcStart = TimeZoneInfo.ConvertTimeToUtc(todayBrazilStart, brazilTimeZone);
        var tomorrowUtcStart = TimeZoneInfo.ConvertTimeToUtc(tomorrowBrazilStart, brazilTimeZone);

        var predictionDeadlineLimit = nowUtc.AddMinutes(PredictionDeadlineMinutes);
        var lastChanceLimit = nowUtc.AddMinutes(LastChanceWindowMinutes);

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.FirebaseUid == firebaseUid, cancellationToken);

        if (user is null)
            throw new UnauthorizedException("Usuário não encontrado.");

        var poolIds = await _context.BolaoMembers
            .AsNoTracking()
            .Where(x => x.UserId == user.Id)
            .Select(x => x.BolaoId)
            .ToListAsync(cancellationToken);

        var totalPredictions = await _context.Predictions
            .AsNoTracking()
            .CountAsync(x => x.UserId == user.Id, cancellationToken);

        var totalPoints = await _context.Predictions
            .AsNoTracking()
            .Where(x => x.UserId == user.Id)
            .SumAsync(x => (int?)x.PointsEarned, cancellationToken) ?? 0;

        var poolsBase = await _context.Boloes
            .AsNoTracking()
            .Where(x => poolIds.Contains(x.Id))
            .Select(x => new
            {
                BolaoId = x.Id,
                BolaoName = x.Name,
                MembersCount = _context.BolaoMembers.Count(m => m.BolaoId == x.Id)
            })
            .ToListAsync(cancellationToken);

        var poolStats = await _context.Predictions
            .AsNoTracking()
            .Where(x => poolIds.Contains(x.BolaoId))
            .GroupBy(x => new { x.BolaoId, x.UserId })
            .Select(x => new
            {
                x.Key.BolaoId,
                x.Key.UserId,
                Points = x.Sum(p => p.PointsEarned),
                PredictionsCount = x.Count(),
                ExactScores = x.Count(p => p.ExactScoreHit)
            })
            .ToListAsync(cancellationToken);

        var pools = poolsBase
            .Select(pool =>
            {
                var ranking = poolStats
                    .Where(x => x.BolaoId == pool.BolaoId)
                    .OrderByDescending(x => x.Points)
                    .ThenByDescending(x => x.ExactScores)
                    .ThenByDescending(x => x.PredictionsCount)
                    .ToList();

                var myStats = ranking.FirstOrDefault(x => x.UserId == user.Id);

                return new HomePoolResponse
                {
                    BolaoId = pool.BolaoId,
                    BolaoName = pool.BolaoName,
                    MembersCount = pool.MembersCount,
                    Position = myStats is null
                        ? pool.MembersCount
                        : ranking.FindIndex(x => x.UserId == user.Id) + 1,
                    Points = myStats?.Points ?? 0,
                    PredictionsCount = myStats?.PredictionsCount ?? 0
                };
            })
            .OrderBy(x => x.Position)
            .ThenByDescending(x => x.Points)
            .Take(MaxHomePools)
            .ToList();

        var predictedMatchIds = await _context.Predictions
            .AsNoTracking()
            .Where(x => x.UserId == user.Id)
            .Select(x => x.FootballMatchId)
            .ToListAsync(cancellationToken);

        var predictedMatchIdsSet = predictedMatchIds.ToHashSet();

        var matches = await _context.FootballMatches
            .AsNoTracking()
            .Where(match =>
                match.MatchDate != null &&
                match.MatchDate >= todayUtcStart &&
                match.MatchDate < tomorrowUtcStart &&
                match.Status != "finalizado")
            .OrderBy(match => match.MatchDate)
            .Take(MaxTodayMatches)
            .ToListAsync(cancellationToken);

        var teamExternalIds = matches
            .SelectMany(match => new[]
            {
                match.HomeTeamExternalId,
                match.AwayTeamExternalId
            })
            .Distinct()
            .ToList();

        var teams = await _context.FootballTeams
            .AsNoTracking()
            .Where(team => teamExternalIds.Contains(team.ExternalId))
            .ToDictionaryAsync(team => team.ExternalId, cancellationToken);

        var todayMatches = matches
            .Select(match =>
            {
                teams.TryGetValue(match.HomeTeamExternalId, out var homeTeam);
                teams.TryGetValue(match.AwayTeamExternalId, out var awayTeam);

                var matchDateUtc = SpecifyUtc(match.MatchDate!.Value);
                var canStillPredict = matchDateUtc > predictionDeadlineLimit;

                return new HomeMatchResponse
                {
                    MatchId = match.Id,
                    Title = match.ScoreText,
                    GroupName = match.GroupName,
                    RoundName = match.RoundName,
                    MatchDate = match.MatchDate,
                    MatchDateText = match.MatchDateText,
                    MatchTimeText = match.MatchTimeText,
                    HasPrediction = predictedMatchIdsSet.Contains(match.Id),
                    LastChance = canStillPredict && matchDateUtc <= lastChanceLimit,

                    HomeTeam = new HomeTeamResponse
                    {
                        ExternalId = match.HomeTeamExternalId,
                        Name = homeTeam?.Name ?? string.Empty,
                        Acronym = homeTeam?.Acronym ?? string.Empty,
                        LogoUrl = homeTeam?.LogoUrl ?? string.Empty
                    },

                    AwayTeam = new HomeTeamResponse
                    {
                        ExternalId = match.AwayTeamExternalId,
                        Name = awayTeam?.Name ?? string.Empty,
                        Acronym = awayTeam?.Acronym ?? string.Empty,
                        LogoUrl = awayTeam?.LogoUrl ?? string.Empty
                    }
                };
            })
            .ToList();

        var pendingMatches = todayMatches
            .Where(x =>
            {
                if (x.MatchDate is null)
                    return false;

                var matchDateUtc = SpecifyUtc(x.MatchDate.Value);

                return !x.HasPrediction && matchDateUtc > predictionDeadlineLimit;
            })
            .Take(MaxSectionItems)
            .ToList();

        var response = new GetHomeResponse
        {
            UserName = user.Name,
            TotalPools = poolIds.Count,
            TotalPredictions = totalPredictions,
            TotalPoints = totalPoints,
            PendingPredictions = pendingMatches.Count,
            Pools = pools,
            LastChanceMatches = todayMatches
                .Where(x => !x.HasPrediction && x.LastChance)
                .Take(MaxSectionItems)
                .ToList(),
            PendingMatches = pendingMatches,
            UpcomingMatches = todayMatches
        };

        _cacheService.Set(cacheKey, response, CacheDuration);

        return response;
    }

    private static DateTime SpecifyUtc(DateTime date)
    {
        return date.Kind == DateTimeKind.Utc
            ? date
            : DateTime.SpecifyKind(date, DateTimeKind.Utc);
    }

    private static TimeZoneInfo GetBrazilTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
        }
        catch
        {
            return TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
        }
    }
}
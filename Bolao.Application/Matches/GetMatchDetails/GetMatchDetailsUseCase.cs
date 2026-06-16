using Bolao.Application.Common.Caching;
using Bolao.Application.Common.Exceptions;
using Bolao.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bolao.Application.Matches.GetMatchDetails;

public sealed class GetMatchDetailsUseCase
{
    private const int PredictionDeadlineMinutes = 15;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(30);

    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;

    public GetMatchDetailsUseCase(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ICacheService cacheService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
    }

    public async Task<GetMatchDetailsResponse> ExecuteAsync(
        GetMatchDetailsQuery query,
        CancellationToken cancellationToken)
    {
        var firebaseUid = _currentUserService.FirebaseUid;

        if (string.IsNullOrWhiteSpace(firebaseUid))
            throw new UnauthorizedException("Usuário não autenticado.");

        var cacheKey = CacheKeys.MatchDetails(
            query.MatchId,
            query.BolaoId,
            firebaseUid);

        var cachedResponse =
            _cacheService.Get<GetMatchDetailsResponse>(cacheKey);

        if (cachedResponse is not null)
            return cachedResponse;

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.FirebaseUid == firebaseUid,
                cancellationToken);

        if (user is null)
            throw new UnauthorizedException("Usuário não encontrado.");

        var isMember = await _context.BolaoMembers
            .AsNoTracking()
            .AnyAsync(
                x => x.BolaoId == query.BolaoId &&
                     x.UserId == user.Id,
                cancellationToken);

        if (!isMember)
            throw new ForbiddenException("Você não participa deste bolão.");

        var match = await _context.FootballMatches
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == query.MatchId,
                cancellationToken);

        if (match is null)
            throw new NotFoundException("Partida não encontrada.");

        var teams = await _context.FootballTeams
            .AsNoTracking()
            .Where(x =>
                x.ExternalId == match.HomeTeamExternalId ||
                x.ExternalId == match.AwayTeamExternalId)
            .ToListAsync(cancellationToken);

        var homeTeam = teams.FirstOrDefault(
            x => x.ExternalId == match.HomeTeamExternalId);

        var awayTeam = teams.FirstOrDefault(
            x => x.ExternalId == match.AwayTeamExternalId);

        var prediction = await _context.Predictions
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.BolaoId == query.BolaoId &&
                     x.UserId == user.Id &&
                     x.FootballMatchId == query.MatchId,
                cancellationToken);

        var predictionDeadline = match.MatchDate?
            .AddMinutes(-PredictionDeadlineMinutes);

        var canPredict =
            match.MatchDate.HasValue &&
            predictionDeadline.HasValue &&
            DateTime.UtcNow < predictionDeadline.Value &&
            match.Status != "finalizado";

        var response = new GetMatchDetailsResponse
        {
            MatchId = match.Id,
            BolaoId = query.BolaoId,
            Title = match.ScoreText,
            GroupName = match.GroupName,
            RoundName = match.RoundName,
            MatchDate = match.MatchDate,
            MatchDateText = match.MatchDateText,
            MatchTimeText = match.MatchTimeText,
            StadiumName = match.StadiumName,
            Status = match.Status,
            HomeScore = match.HomeScore,
            AwayScore = match.AwayScore,
            CanPredict = canPredict,
            PredictionDeadline = predictionDeadline,

            HomeTeam = new TeamDetailsResponse
            {
                ExternalId = match.HomeTeamExternalId,
                Name = homeTeam?.Name ?? string.Empty,
                Acronym = homeTeam?.Acronym ?? string.Empty,
                LogoUrl = homeTeam?.LogoUrl ?? string.Empty
            },

            AwayTeam = new TeamDetailsResponse
            {
                ExternalId = match.AwayTeamExternalId,
                Name = awayTeam?.Name ?? string.Empty,
                Acronym = awayTeam?.Acronym ?? string.Empty,
                LogoUrl = awayTeam?.LogoUrl ?? string.Empty
            },

            MyPrediction = prediction is null
                ? null
                : new MyPredictionResponse
                {
                    PredictionId = prediction.Id,
                    HomeScore = prediction.HomeScore,
                    AwayScore = prediction.AwayScore,
                    PointsEarned = prediction.PointsEarned,
                    ExactScoreHit = prediction.ExactScoreHit,
                    WinnerHit = prediction.WinnerHit
                }
        };

        _cacheService.Set(
            cacheKey,
            response,
            CacheDuration);

        return response;
    }
}
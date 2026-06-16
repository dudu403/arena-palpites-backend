using Bolao.Application.Common.Exceptions;
using Bolao.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bolao.Application.Predictions.GetPredictionHistory;

public sealed class GetPredictionHistoryUseCase
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetPredictionHistoryUseCase(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<GetPredictionHistoryResponse>> ExecuteAsync(
        GetPredictionHistoryQuery query,
        CancellationToken cancellationToken)
    {
        var firebaseUid = _currentUserService.FirebaseUid;

        if (string.IsNullOrWhiteSpace(firebaseUid))
            throw new UnauthorizedException("Usuário não autenticado.");

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

        var data = await _context.Predictions
            .AsNoTracking()
            .Where(prediction =>
                prediction.BolaoId == query.BolaoId &&
                prediction.UserId == user.Id)
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

        return data
            .Select(x =>
            {
                var teams = SplitScoreText(x.Match.ScoreText);

                return new GetPredictionHistoryResponse
                {
                    PredictionId = x.Prediction.Id,
                    FootballMatchId = x.Match.Id,

                    HomeTeamName = teams.HomeTeamName,
                    AwayTeamName = teams.AwayTeamName,

                    PredictedHomeScore = x.Prediction.HomeScore,
                    PredictedAwayScore = x.Prediction.AwayScore,

                    RealHomeScore = x.Match.HomeScore,
                    RealAwayScore = x.Match.AwayScore,

                    MatchStatus = x.Match.Status,
                    MatchDate = x.Match.MatchDate,
                    MatchDateText = x.Match.MatchDateText,
                    MatchTimeText = x.Match.MatchTimeText,

                    PointsEarned = x.Prediction.PointsEarned,
                    ExactScoreHit = x.Prediction.ExactScoreHit,
                    WinnerHit = x.Prediction.WinnerHit
                };
            })
            .ToList();
    }

    private static (string HomeTeamName, string AwayTeamName) SplitScoreText(
        string scoreText)
    {
        if (string.IsNullOrWhiteSpace(scoreText))
            return (string.Empty, string.Empty);

        var parts = scoreText.Split(
            " x ",
            StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 2)
            return (scoreText, string.Empty);

        return (parts[0], parts[1]);
    }
}
using Bolao.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bolao.Application.Predictions.GetMyPredictions;

public sealed class GetMyPredictionsUseCase
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetMyPredictionsUseCase(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<GetMyPredictionsResponse>> ExecuteAsync(
        GetMyPredictionsQuery query,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_currentUserService.FirebaseUid))
            throw new Exception("Usuário não autenticado.");

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.FirebaseUid == _currentUserService.FirebaseUid,
                cancellationToken);

        if (user is null)
            throw new Exception("Usuário não encontrado.");

        var isMember = await _context.BolaoMembers
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.BolaoId == query.BolaoId &&
                    x.UserId == user.Id,
                cancellationToken);

        if (!isMember)
            throw new Exception("Você não participa deste bolão.");

        var predictions = await _context.Predictions
            .AsNoTracking()
            .Where(x =>
                x.BolaoId == query.BolaoId &&
                x.UserId == user.Id)
            .OrderBy(x => x.CreatedAt)
            .Select(x => new GetMyPredictionsResponse
            {
                Id = x.Id,
                BolaoId = x.BolaoId,
                FootballMatchId = x.FootballMatchId,
                HomeScore = x.HomeScore,
                AwayScore = x.AwayScore,
                PointsEarned = x.PointsEarned,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return predictions;
    }
}
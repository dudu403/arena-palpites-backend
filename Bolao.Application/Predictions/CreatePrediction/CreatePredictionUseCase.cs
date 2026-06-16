using Bolao.Application.Common.Caching;
using Bolao.Application.Common.Exceptions;
using Bolao.Application.Common.Interfaces;
using Bolao.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bolao.Application.Predictions.CreatePrediction;

public sealed class CreatePredictionUseCase
{
    private const int PredictionDeadlineMinutes = 15;

    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;

    public CreatePredictionUseCase(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ICacheService cacheService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
    }

    public async Task<CreatePredictionResponse> ExecuteAsync(
        CreatePredictionCommand command,
        CancellationToken cancellationToken)
    {
        var firebaseUid = _currentUserService.FirebaseUid;

        if (string.IsNullOrWhiteSpace(firebaseUid))
            throw new UnauthorizedException("Usuário não autenticado.");

        var user = await _context.Users
            .FirstOrDefaultAsync(
                x => x.FirebaseUid == firebaseUid,
                cancellationToken);

        if (user is null)
            throw new UnauthorizedException("Usuário não encontrado.");

        var bolaoExists = await _context.Boloes
            .AnyAsync(
                x => x.Id == command.BolaoId,
                cancellationToken);

        if (!bolaoExists)
            throw new NotFoundException("Bolão não encontrado.");

        var isMember = await _context.BolaoMembers
            .AnyAsync(
                x =>
                    x.BolaoId == command.BolaoId &&
                    x.UserId == user.Id,
                cancellationToken);

        if (!isMember)
            throw new ForbiddenException("Você não participa deste bolão.");

        var match = await _context.FootballMatches
            .FirstOrDefaultAsync(
                x => x.Id == command.FootballMatchId,
                cancellationToken);

        if (match is null)
            throw new NotFoundException("Partida não encontrada.");

        if (match.MatchDate is null)
            throw new BadRequestException("Esta partida ainda não possui data definida.");

        var predictionDeadline = match.MatchDate.Value
            .AddMinutes(-PredictionDeadlineMinutes);

        if (DateTime.UtcNow >= predictionDeadline)
            throw new BadRequestException("Palpites para esta partida estão encerrados.");

        var prediction = await _context.Predictions
            .FirstOrDefaultAsync(
                x =>
                    x.BolaoId == command.BolaoId &&
                    x.UserId == user.Id &&
                    x.FootballMatchId == command.FootballMatchId,
                cancellationToken);

        if (prediction is null)
        {
            prediction = new Prediction
            {
                BolaoId = command.BolaoId,
                UserId = user.Id,
                FootballMatchId = command.FootballMatchId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Predictions.Add(prediction);
        }

        prediction.HomeScore = command.HomeScore;
        prediction.AwayScore = command.AwayScore;
        prediction.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _cacheService.Remove(CacheKeys.Home(firebaseUid));
        _cacheService.Remove(CacheKeys.BolaoRanking(command.BolaoId));
        _cacheService.Remove(CacheKeys.BolaoDashboard(command.BolaoId, firebaseUid));
        _cacheService.Remove(CacheKeys.MatchDetails(
            command.FootballMatchId,
            command.BolaoId,
            firebaseUid));

        return new CreatePredictionResponse
        {
            Id = prediction.Id,
            BolaoId = prediction.BolaoId,
            FootballMatchId = prediction.FootballMatchId,
            HomeScore = prediction.HomeScore,
            AwayScore = prediction.AwayScore,
            PointsEarned = prediction.PointsEarned
        };
    }
}
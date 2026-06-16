using Bolao.Application.Common.Caching;
using Bolao.Application.Common.Interfaces;
using Bolao.Application.Common.Results;
using Bolao.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bolao.Application.Boloes.JoinBolao;

public sealed class JoinBolaoUseCase
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;

    public JoinBolaoUseCase(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ICacheService cacheService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
    }

    public async Task<Result<JoinBolaoResponse>> ExecuteAsync(
        JoinBolaoCommand command,
        CancellationToken cancellationToken = default)
    {
        var firebaseUid = _currentUserService.FirebaseUid;

        if (string.IsNullOrWhiteSpace(firebaseUid))
            return Result<JoinBolaoResponse>.Failure("Usuário não autenticado.", 401);

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.FirebaseUid == firebaseUid, cancellationToken);

        if (user == null)
            return Result<JoinBolaoResponse>.Failure("Usuário não encontrado.", 401);

        if (!user.IsActive)
            return Result<JoinBolaoResponse>.Failure("Usuário inativo.", 401);

        var inviteCode = command.InviteCode.Trim().ToUpperInvariant();

        var bolao = await _context.Boloes
            .AsNoTracking()
            .Where(x =>
                x.Privacy == Domain.Entities.Bolao.PrivatePrivacy &&
                x.InviteCode == inviteCode)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.Championship,
                x.Description,
                x.MaxParticipants,
                x.Privacy,
                x.InviteCode,
                x.OwnerId,
                x.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (bolao == null)
            return Result<JoinBolaoResponse>.Failure("Bolão não encontrado para este código.", 404);

        var alreadyMember = await _context.BolaoMembers
            .AsNoTracking()
            .AnyAsync(x => x.BolaoId == bolao.Id && x.UserId == user.Id, cancellationToken);

        if (alreadyMember)
            return Result<JoinBolaoResponse>.Failure("Você já participa deste bolão.", 409);

        var currentMembers = await _context.BolaoMembers
            .AsNoTracking()
            .CountAsync(x => x.BolaoId == bolao.Id, cancellationToken);

        if (currentMembers >= bolao.MaxParticipants)
            return Result<JoinBolaoResponse>.Failure("Este bolão atingiu o limite de participantes.", 409);

        var member = new BolaoMember(
            bolao.Id,
            user.Id,
            BolaoMember.MemberRole
        );

        _context.BolaoMembers.Add(member);

        await _context.SaveChangesAsync(cancellationToken);

        _cacheService.Remove(CacheKeys.Home(firebaseUid));
        _cacheService.Remove(CacheKeys.BolaoRanking(bolao.Id));
        _cacheService.Remove(CacheKeys.BolaoDashboard(bolao.Id, firebaseUid));

        return Result<JoinBolaoResponse>.Success(new JoinBolaoResponse
        {
            Id = bolao.Id,
            Name = bolao.Name,
            Championship = bolao.Championship,
            Description = bolao.Description,
            MaxParticipants = bolao.MaxParticipants,
            Privacy = bolao.Privacy,
            InviteCode = bolao.InviteCode,
            Role = BolaoMember.MemberRole,
            JoinedAt = member.JoinedAt,
            CreatedAt = bolao.CreatedAt
        });
    }
}
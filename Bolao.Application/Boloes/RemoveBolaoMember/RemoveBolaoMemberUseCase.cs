using Bolao.Application.Common.Caching;
using Bolao.Application.Common.Interfaces;
using Bolao.Application.Common.Results;
using Bolao.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bolao.Application.Boloes.RemoveBolaoMember;

public sealed class RemoveBolaoMemberUseCase
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;

    public RemoveBolaoMemberUseCase(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ICacheService cacheService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
    }

    public async Task<Result<RemoveBolaoMemberResponse>> ExecuteAsync(
        RemoveBolaoMemberCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.BolaoId == Guid.Empty)
            return Result<RemoveBolaoMemberResponse>.Failure("Bolão inválido.", 400);

        if (command.UserId == Guid.Empty)
            return Result<RemoveBolaoMemberResponse>.Failure("Usuário inválido.", 400);

        var firebaseUid = _currentUserService.FirebaseUid;

        if (string.IsNullOrWhiteSpace(firebaseUid))
            return Result<RemoveBolaoMemberResponse>.Failure("Usuário não autenticado.", 401);

        var currentUser = await _context.Users
            .AsNoTracking()
            .Where(x => x.FirebaseUid == firebaseUid)
            .Select(x => new { x.Id, x.IsActive })
            .FirstOrDefaultAsync(cancellationToken);

        if (currentUser == null)
            return Result<RemoveBolaoMemberResponse>.Failure("Usuário não encontrado.", 401);

        if (!currentUser.IsActive)
            return Result<RemoveBolaoMemberResponse>.Failure("Usuário inativo.", 401);

        var currentMember = await _context.BolaoMembers
            .AsNoTracking()
            .Where(x => x.BolaoId == command.BolaoId && x.UserId == currentUser.Id)
            .Select(x => new { x.Role })
            .FirstOrDefaultAsync(cancellationToken);

        if (currentMember == null)
            return Result<RemoveBolaoMemberResponse>.Failure("Bolão não encontrado.", 404);

        if (currentMember.Role != BolaoMember.OwnerRole)
            return Result<RemoveBolaoMemberResponse>.Failure(
                "Você não tem permissão para remover participantes.",
                403);

        if (currentUser.Id == command.UserId)
            return Result<RemoveBolaoMemberResponse>.Failure(
                "O dono não pode remover a si mesmo.",
                409);

        var memberToRemove = await _context.BolaoMembers
            .FirstOrDefaultAsync(
                x => x.BolaoId == command.BolaoId && x.UserId == command.UserId,
                cancellationToken);

        if (memberToRemove == null)
            return Result<RemoveBolaoMemberResponse>.Failure("Participante não encontrado.", 404);

        if (memberToRemove.Role == BolaoMember.OwnerRole)
            return Result<RemoveBolaoMemberResponse>.Failure(
                "O dono do bolão não pode ser removido.",
                409);

        var removedUserFirebaseUid = await _context.Users
            .AsNoTracking()
            .Where(x => x.Id == command.UserId)
            .Select(x => x.FirebaseUid)
            .FirstOrDefaultAsync(cancellationToken);

        _context.BolaoMembers.Remove(memberToRemove);

        await _context.SaveChangesAsync(cancellationToken);

        _cacheService.Remove(CacheKeys.Home(firebaseUid));
        _cacheService.Remove(CacheKeys.BolaoRanking(command.BolaoId));
        _cacheService.Remove(CacheKeys.BolaoDashboard(command.BolaoId, firebaseUid));

        if (!string.IsNullOrWhiteSpace(removedUserFirebaseUid))
        {
            _cacheService.Remove(CacheKeys.Home(removedUserFirebaseUid));
            _cacheService.Remove(CacheKeys.BolaoDashboard(command.BolaoId, removedUserFirebaseUid));
        }

        return Result<RemoveBolaoMemberResponse>.Success(new RemoveBolaoMemberResponse
        {
            BolaoId = command.BolaoId,
            RemovedUserId = command.UserId,
            Message = "Participante removido com sucesso."
        });
    }
}
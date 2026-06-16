using Bolao.Application.Common.Caching;
using Bolao.Application.Common.Interfaces;
using Bolao.Application.Common.Results;
using Bolao.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bolao.Application.Boloes.LeaveBolao;

public sealed class LeaveBolaoUseCase
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;

    public LeaveBolaoUseCase(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ICacheService cacheService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
    }

    public async Task<Result<LeaveBolaoResponse>> ExecuteAsync(
        LeaveBolaoCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.BolaoId == Guid.Empty)
            return Result<LeaveBolaoResponse>.Failure("Bolão inválido.", 400);

        var firebaseUid = _currentUserService.FirebaseUid;

        if (string.IsNullOrWhiteSpace(firebaseUid))
            return Result<LeaveBolaoResponse>.Failure("Usuário não autenticado.", 401);

        var user = await _context.Users
            .AsNoTracking()
            .Where(x => x.FirebaseUid == firebaseUid)
            .Select(x => new { x.Id, x.IsActive })
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
            return Result<LeaveBolaoResponse>.Failure("Usuário não encontrado.", 401);

        if (!user.IsActive)
            return Result<LeaveBolaoResponse>.Failure("Usuário inativo.", 401);

        var member = await _context.BolaoMembers
            .FirstOrDefaultAsync(
                x => x.BolaoId == command.BolaoId && x.UserId == user.Id,
                cancellationToken);

        if (member == null)
            return Result<LeaveBolaoResponse>.Failure("Bolão não encontrado.", 404);

        if (member.Role == BolaoMember.OwnerRole)
            return Result<LeaveBolaoResponse>.Failure(
                "O dono não pode sair do bolão. Exclua o bolão ou transfira a propriedade futuramente.",
                409);

        _context.BolaoMembers.Remove(member);

        await _context.SaveChangesAsync(cancellationToken);

        _cacheService.Remove(CacheKeys.Home(firebaseUid));
        _cacheService.Remove(CacheKeys.BolaoRanking(command.BolaoId));
        _cacheService.Remove(CacheKeys.BolaoDashboard(command.BolaoId, firebaseUid));

        return Result<LeaveBolaoResponse>.Success(new LeaveBolaoResponse
        {
            BolaoId = command.BolaoId,
            Message = "Você saiu do bolão com sucesso."
        });
    }
}
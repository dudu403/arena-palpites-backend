using Bolao.Application.Common.Caching;
using Bolao.Application.Common.Interfaces;
using Bolao.Application.Common.Results;
using Bolao.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bolao.Application.Boloes.DeleteBolao;

public sealed class DeleteBolaoUseCase
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;

    public DeleteBolaoUseCase(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ICacheService cacheService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
    }

    public async Task<Result<DeleteBolaoResponse>> ExecuteAsync(
        DeleteBolaoCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.BolaoId == Guid.Empty)
            return Result<DeleteBolaoResponse>.Failure("Bolão inválido.", 400);

        var firebaseUid = _currentUserService.FirebaseUid;

        if (string.IsNullOrWhiteSpace(firebaseUid))
            return Result<DeleteBolaoResponse>.Failure("Usuário não autenticado.", 401);

        var user = await _context.Users
            .AsNoTracking()
            .Where(x => x.FirebaseUid == firebaseUid)
            .Select(x => new { x.Id, x.IsActive })
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
            return Result<DeleteBolaoResponse>.Failure("Usuário não encontrado.", 401);

        if (!user.IsActive)
            return Result<DeleteBolaoResponse>.Failure("Usuário inativo.", 401);

        var ownerMember = await _context.BolaoMembers
            .AsNoTracking()
            .Where(x => x.BolaoId == command.BolaoId && x.UserId == user.Id)
            .Select(x => new { x.Role })
            .FirstOrDefaultAsync(cancellationToken);

        if (ownerMember == null)
            return Result<DeleteBolaoResponse>.Failure("Bolão não encontrado.", 404);

        if (ownerMember.Role != BolaoMember.OwnerRole)
            return Result<DeleteBolaoResponse>.Failure(
                "Você não tem permissão para excluir este bolão.",
                403);

        var bolao = await _context.Boloes
            .FirstOrDefaultAsync(
                x => x.Id == command.BolaoId,
                cancellationToken);

        if (bolao == null)
            return Result<DeleteBolaoResponse>.Failure("Bolão não encontrado.", 404);

        _context.Boloes.Remove(bolao);

        await _context.SaveChangesAsync(cancellationToken);

        _cacheService.Remove(CacheKeys.Home(firebaseUid));
        _cacheService.Remove(CacheKeys.BolaoRanking(command.BolaoId));
        _cacheService.Remove(CacheKeys.BolaoDashboard(command.BolaoId, firebaseUid));

        return Result<DeleteBolaoResponse>.Success(new DeleteBolaoResponse
        {
            BolaoId = command.BolaoId,
            Message = "Bolão excluído com sucesso."
        });
    }
}
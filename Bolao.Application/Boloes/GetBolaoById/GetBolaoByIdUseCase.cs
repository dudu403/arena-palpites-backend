using Bolao.Application.Common.Interfaces;
using Bolao.Application.Common.Results;
using Microsoft.EntityFrameworkCore;

namespace Bolao.Application.Boloes.GetBolaoById;

public sealed class GetBolaoByIdUseCase
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetBolaoByIdUseCase(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<GetBolaoByIdResponse>> ExecuteAsync(
        GetBolaoByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.Id == Guid.Empty)
            return Result<GetBolaoByIdResponse>.Failure("Bolão inválido.", 400);

        var firebaseUid = _currentUserService.FirebaseUid;

        if (string.IsNullOrWhiteSpace(firebaseUid))
            return Result<GetBolaoByIdResponse>.Failure("Usuário não autenticado.", 401);

        var user = await _context.Users
            .AsNoTracking()
            .Where(x => x.FirebaseUid == firebaseUid)
            .Select(x => new
            {
                x.Id,
                x.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
            return Result<GetBolaoByIdResponse>.Failure("Usuário não encontrado.", 401);

        if (!user.IsActive)
            return Result<GetBolaoByIdResponse>.Failure("Usuário inativo.", 401);

        var bolao = await _context.BolaoMembers
            .AsNoTracking()
            .Where(member => member.BolaoId == query.Id && member.UserId == user.Id)
            .Select(member => new GetBolaoByIdResponse
            {
                Id = member.Bolao.Id,
                Name = member.Bolao.Name,
                Championship = member.Bolao.Championship,
                Description = member.Bolao.Description,
                MaxParticipants = member.Bolao.MaxParticipants,
                Privacy = member.Bolao.Privacy,
                InviteCode = member.Bolao.InviteCode,
                OwnerId = member.Bolao.OwnerId,
                Role = member.Role,
                Rules = new GetBolaoByIdRulesResponse
                {
                    ExactScorePoints = member.Bolao.Rules.ExactScorePoints,
                    WinnerPoints = member.Bolao.Rules.WinnerPoints,
                    DrawPoints = member.Bolao.Rules.DrawPoints
                }
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (bolao == null)
            return Result<GetBolaoByIdResponse>.Failure("Bolão não encontrado.", 404);

        return Result<GetBolaoByIdResponse>.Success(bolao);
    }
}
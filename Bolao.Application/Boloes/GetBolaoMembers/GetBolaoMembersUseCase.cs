using Bolao.Application.Common.Interfaces;
using Bolao.Application.Common.Results;
using Microsoft.EntityFrameworkCore;

namespace Bolao.Application.Boloes.GetBolaoMembers;

public sealed class GetBolaoMembersUseCase
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetBolaoMembersUseCase(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<IReadOnlyList<GetBolaoMembersResponse>>> ExecuteAsync(
        GetBolaoMembersQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.BolaoId == Guid.Empty)
            return Result<IReadOnlyList<GetBolaoMembersResponse>>.Failure("Bolão inválido.", 400);

        var firebaseUid = _currentUserService.FirebaseUid;

        if (string.IsNullOrWhiteSpace(firebaseUid))
            return Result<IReadOnlyList<GetBolaoMembersResponse>>.Failure("Usuário não autenticado.", 401);

        var currentUser = await _context.Users
            .AsNoTracking()
            .Where(x => x.FirebaseUid == firebaseUid)
            .Select(x => new
            {
                x.Id,
                x.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (currentUser == null)
            return Result<IReadOnlyList<GetBolaoMembersResponse>>.Failure("Usuário não encontrado.", 401);

        if (!currentUser.IsActive)
            return Result<IReadOnlyList<GetBolaoMembersResponse>>.Failure("Usuário inativo.", 401);

        var isMember = await _context.BolaoMembers
            .AsNoTracking()
            .AnyAsync(x => x.BolaoId == query.BolaoId && x.UserId == currentUser.Id, cancellationToken);

        if (!isMember)
            return Result<IReadOnlyList<GetBolaoMembersResponse>>.Failure("Bolão não encontrado.", 404);

        var members = await _context.BolaoMembers
            .AsNoTracking()
            .Where(x => x.BolaoId == query.BolaoId)
            .OrderBy(x => x.Role == "Owner" ? 0 : 1)
            .ThenBy(x => x.JoinedAt)
            .Select(x => new GetBolaoMembersResponse
            {
                UserId = x.User.Id,
                Name = x.User.Name,
                Email = x.User.Email,
                PhotoUrl = x.User.PhotoUrl,
                Role = x.Role,
                JoinedAt = x.JoinedAt
            })
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<GetBolaoMembersResponse>>.Success(members);
    }
}
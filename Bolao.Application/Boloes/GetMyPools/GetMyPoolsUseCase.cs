using Bolao.Application.Common.Interfaces;
using Bolao.Application.Common.Results;
using Microsoft.EntityFrameworkCore;

namespace Bolao.Application.Boloes.GetMyPools;

public sealed class GetMyPoolsUseCase
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetMyPoolsUseCase(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<GetMyPoolsResponse>> ExecuteAsync(
        GetMyPoolsQuery query,
        CancellationToken cancellationToken = default)
    {
        var firebaseUid = _currentUserService.FirebaseUid;

        if (string.IsNullOrWhiteSpace(firebaseUid))
            return Result<GetMyPoolsResponse>.Failure("Usuário não autenticado.", 401);

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
            return Result<GetMyPoolsResponse>.Failure("Usuário não encontrado.", 401);

        if (!user.IsActive)
            return Result<GetMyPoolsResponse>.Failure("Usuário inativo.", 401);

        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 50);

        var baseQuery = _context.BolaoMembers
            .AsNoTracking()
            .Where(member => member.UserId == user.Id)
            .OrderByDescending(member => member.JoinedAt)
            .Select(member => new MyPoolSummaryResponse
            {
                Id = member.Bolao.Id,
                Name = member.Bolao.Name,
                Championship = member.Bolao.Championship,
                Description = member.Bolao.Description,
                MaxParticipants = member.Bolao.MaxParticipants,
                Privacy = member.Bolao.Privacy,
                InviteCode = member.Bolao.InviteCode,
                Role = member.Role,
                JoinedAt = member.JoinedAt,
                CreatedAt = member.Bolao.CreatedAt
            });

        var totalItems = await baseQuery.CountAsync(cancellationToken);

        var items = await baseQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var response = new GetMyPoolsResponse
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
            Items = items
        };

        return Result<GetMyPoolsResponse>.Success(response);
    }
}
using Bolao.Application.Common.Caching;
using Bolao.Application.Common.Interfaces;
using Bolao.Application.Common.Results;
using Bolao.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bolao.Application.Boloes.CreateBolao;

public sealed class CreateBolaoUseCase
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IInviteCodeGenerator _inviteCodeGenerator;
    private readonly ICacheService _cacheService;

    public CreateBolaoUseCase(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IInviteCodeGenerator inviteCodeGenerator,
        ICacheService cacheService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _inviteCodeGenerator = inviteCodeGenerator;
        _cacheService = cacheService;
    }

    public async Task<Result<CreateBolaoResponse>> ExecuteAsync(
        CreateBolaoCommand command,
        CancellationToken cancellationToken = default)
    {
        var firebaseUid = _currentUserService.FirebaseUid;

        if (string.IsNullOrWhiteSpace(firebaseUid))
            return Result<CreateBolaoResponse>.Failure(
                "Usuário não autenticado.",
                401);

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.FirebaseUid == firebaseUid,
                cancellationToken);

        if (user == null)
            return Result<CreateBolaoResponse>.Failure(
                "Usuário não encontrado.",
                401);

        if (!user.IsActive)
            return Result<CreateBolaoResponse>.Failure(
                "Usuário inativo.",
                401);

        var championship = await _context.Championships
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.ExternalId == command.ChampionshipExternalId,
                cancellationToken);

        if (championship is null)
            return Result<CreateBolaoResponse>.Failure(
                "Campeonato não encontrado.",
                404);

        var championshipName = string.IsNullOrWhiteSpace(championship.PopularName)
            ? championship.Name
            : championship.PopularName;

        var inviteCode = command.Privacy == Domain.Entities.Bolao.PrivatePrivacy
            ? await _inviteCodeGenerator.GenerateUniqueCodeAsync(cancellationToken)
            : null;

        var rules = new BolaoRules(
            command.Rules.ExactScorePoints,
            command.Rules.WinnerPoints,
            command.Rules.DrawPoints
        );

        var bolao = new Domain.Entities.Bolao(
            command.Name,
            championshipName,
            championship.ExternalId,
            command.Description,
            command.MaxParticipants,
            command.Privacy,
            user.Id,
            inviteCode,
            rules
        );

        _context.Boloes.Add(bolao);

        _context.BolaoMembers.Add(new BolaoMember(
            bolao.Id,
            user.Id,
            BolaoMember.OwnerRole
        ));

        await _context.SaveChangesAsync(cancellationToken);

        _cacheService.Remove(CacheKeys.Home(firebaseUid));

        var response = new CreateBolaoResponse
        {
            Id = bolao.Id,
            Name = bolao.Name,
            Championship = bolao.Championship,
            ChampionshipExternalId = bolao.ChampionshipExternalId,
            Description = bolao.Description,
            MaxParticipants = bolao.MaxParticipants,
            Privacy = bolao.Privacy,
            InviteCode = bolao.InviteCode,
            OwnerId = bolao.OwnerId,
            Rules = new CreateBolaoRulesResponse
            {
                ExactScorePoints = rules.ExactScorePoints,
                WinnerPoints = rules.WinnerPoints,
                DrawPoints = rules.DrawPoints
            }
        };

        return Result<CreateBolaoResponse>.Success(response, 201);
    }
}
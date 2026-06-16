using Bolao.Api.Contracts.Boloes;
using Bolao.Application.Boloes.CreateBolao;
using Bolao.Application.Boloes.DeleteBolao;
using Bolao.Application.Boloes.GetBolaoById;
using Bolao.Application.Boloes.GetBolaoCurrentRoundMatches;
using Bolao.Application.Boloes.GetBolaoDashboard;
using Bolao.Application.Boloes.GetBolaoMatches;
using Bolao.Application.Boloes.GetBolaoMembers;
using Bolao.Application.Boloes.GetBolaoRanking;
using Bolao.Application.Boloes.GetMemberFinishedPredictions;
using Bolao.Application.Boloes.GetMyPools;
using Bolao.Application.Boloes.JoinBolao;
using Bolao.Application.Boloes.LeaveBolao;
using Bolao.Application.Boloes.RemoveBolaoMember;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Bolao.Api.Controllers;

[ApiController]
[Route("api/boloes")]
[Authorize]
public class BoloesController : ControllerBase
{
    private readonly CreateBolaoUseCase _createBolaoUseCase;
    private readonly JoinBolaoUseCase _joinBolaoUseCase;
    private readonly GetMyPoolsUseCase _getMyPoolsUseCase;
    private readonly GetBolaoByIdUseCase _getBolaoByIdUseCase;
    private readonly GetBolaoMembersUseCase _getBolaoMembersUseCase;
    private readonly GetBolaoRankingUseCase _getBolaoRankingUseCase;
    private readonly GetBolaoDashboardUseCase _getBolaoDashboardUseCase;
    private readonly GetBolaoMatchesUseCase _getBolaoMatchesUseCase;
    private readonly GetBolaoCurrentRoundMatchesUseCase _getBolaoCurrentRoundMatchesUseCase;
    private readonly GetMemberFinishedPredictionsUseCase _getMemberFinishedPredictionsUseCase;
    private readonly LeaveBolaoUseCase _leaveBolaoUseCase;
    private readonly DeleteBolaoUseCase _deleteBolaoUseCase;
    private readonly RemoveBolaoMemberUseCase _removeBolaoMemberUseCase;

    public BoloesController(
        CreateBolaoUseCase createBolaoUseCase,
        JoinBolaoUseCase joinBolaoUseCase,
        GetMyPoolsUseCase getMyPoolsUseCase,
        GetBolaoByIdUseCase getBolaoByIdUseCase,
        GetBolaoMembersUseCase getBolaoMembersUseCase,
        GetBolaoRankingUseCase getBolaoRankingUseCase,
        GetBolaoDashboardUseCase getBolaoDashboardUseCase,
        GetBolaoMatchesUseCase getBolaoMatchesUseCase,
        GetBolaoCurrentRoundMatchesUseCase getBolaoCurrentRoundMatchesUseCase,
        GetMemberFinishedPredictionsUseCase getMemberFinishedPredictionsUseCase,
        LeaveBolaoUseCase leaveBolaoUseCase,
        DeleteBolaoUseCase deleteBolaoUseCase,
        RemoveBolaoMemberUseCase removeBolaoMemberUseCase)
    {
        _createBolaoUseCase = createBolaoUseCase;
        _joinBolaoUseCase = joinBolaoUseCase;
        _getMyPoolsUseCase = getMyPoolsUseCase;
        _getBolaoByIdUseCase = getBolaoByIdUseCase;
        _getBolaoMembersUseCase = getBolaoMembersUseCase;
        _getBolaoRankingUseCase = getBolaoRankingUseCase;
        _getBolaoDashboardUseCase = getBolaoDashboardUseCase;
        _getBolaoMatchesUseCase = getBolaoMatchesUseCase;
        _getBolaoCurrentRoundMatchesUseCase = getBolaoCurrentRoundMatchesUseCase;
        _getMemberFinishedPredictionsUseCase = getMemberFinishedPredictionsUseCase;
        _leaveBolaoUseCase = leaveBolaoUseCase;
        _deleteBolaoUseCase = deleteBolaoUseCase;
        _removeBolaoMemberUseCase = removeBolaoMemberUseCase;
    }

    [HttpPost]
    [EnableRateLimiting("CreatePool")]
    public async Task<IActionResult> Create(
        [FromBody] CreateBolaoRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateBolaoCommand
        {
            Name = request.Name,
            Championship = request.Championship,
            ChampionshipExternalId = request.ChampionshipExternalId,
            Description = request.Description,
            MaxParticipants = request.MaxParticipants,
            Privacy = request.Privacy,
            Rules = new CreateBolaoRulesCommand
            {
                ExactScorePoints = request.Rules.ExactScorePoints,
                WinnerPoints = request.Rules.WinnerPoints,
                DrawPoints = request.Rules.DrawPoints
            }
        };

        var result = await _createBolaoUseCase.ExecuteAsync(command, cancellationToken);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result.Error);

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPost("join")]
    [EnableRateLimiting("Sensitive")]
    public async Task<IActionResult> Join(
        [FromBody] JoinBolaoRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _joinBolaoUseCase.ExecuteAsync(
            new JoinBolaoCommand { InviteCode = request.InviteCode },
            cancellationToken);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result.Error);

        return Ok(result.Value);
    }

    [HttpGet("me")]
    [EnableRateLimiting("General")]
    public async Task<IActionResult> GetMyPools(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _getMyPoolsUseCase.ExecuteAsync(
            new GetMyPoolsQuery { Page = page, PageSize = pageSize },
            cancellationToken);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result.Error);

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    [EnableRateLimiting("General")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _getBolaoByIdUseCase.ExecuteAsync(
            new GetBolaoByIdQuery { Id = id },
            cancellationToken);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result.Error);

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}/dashboard")]
    [EnableRateLimiting("General")]
    public async Task<IActionResult> GetDashboard(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _getBolaoDashboardUseCase.ExecuteAsync(
            new GetBolaoDashboardQuery { BolaoId = id },
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}/matches")]
    [EnableRateLimiting("General")]
    public async Task<IActionResult> GetMatches(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _getBolaoMatchesUseCase.ExecuteAsync(
            new GetBolaoMatchesQuery { BolaoId = id },
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}/matches/current-round")]
    [EnableRateLimiting("General")]
    public async Task<IActionResult> GetCurrentRoundMatches(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _getBolaoCurrentRoundMatchesUseCase.ExecuteAsync(
            new GetBolaoCurrentRoundMatchesQuery { BolaoId = id },
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}/members")]
    [EnableRateLimiting("General")]
    public async Task<IActionResult> GetMembers(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _getBolaoMembersUseCase.ExecuteAsync(
            new GetBolaoMembersQuery { BolaoId = id },
            cancellationToken);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result.Error);

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}/ranking")]
    [EnableRateLimiting("General")]
    public async Task<IActionResult> GetRanking(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _getBolaoRankingUseCase.ExecuteAsync(
            new GetBolaoRankingQuery { BolaoId = id },
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}/members/{userId:guid}/predictions/finished")]
    [EnableRateLimiting("General")]
    public async Task<IActionResult> GetMemberFinishedPredictions(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var result = await _getMemberFinishedPredictionsUseCase.ExecuteAsync(
            new GetMemberFinishedPredictionsQuery
            {
                BolaoId = id,
                UserId = userId
            },
            cancellationToken);

        return Ok(result);
    }

    [HttpDelete("{id:guid}/leave")]
    [EnableRateLimiting("Sensitive")]
    public async Task<IActionResult> Leave(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _leaveBolaoUseCase.ExecuteAsync(
            new LeaveBolaoCommand { BolaoId = id },
            cancellationToken);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result.Error);

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    [EnableRateLimiting("Sensitive")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _deleteBolaoUseCase.ExecuteAsync(
            new DeleteBolaoCommand { BolaoId = id },
            cancellationToken);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result.Error);

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}/members/{userId:guid}")]
    [EnableRateLimiting("Sensitive")]
    public async Task<IActionResult> RemoveMember(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var result = await _removeBolaoMemberUseCase.ExecuteAsync(
            new RemoveBolaoMemberCommand
            {
                BolaoId = id,
                UserId = userId
            },
            cancellationToken);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result.Error);

        return Ok(result.Value);
    }
}
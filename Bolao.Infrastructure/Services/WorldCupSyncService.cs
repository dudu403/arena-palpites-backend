using System.Globalization;
using System.Text.Json;
using Bolao.Application.Common.Interfaces;
using Bolao.Application.WorldCup.SyncWorldCup;
using Bolao.Domain.Entities;
using Bolao.Infrastructure.ExternalServices.ApiFutebol;
using Bolao.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bolao.Infrastructure.Services;

public sealed class WorldCupSyncService : IWorldCupSyncService
{
    private const int WorldCupChampionshipId = 72;
    private const int GroupStagePhaseId = 886;

    private readonly AppDbContext _context;
    private readonly IApiFutebolService _apiFutebolService;

    public WorldCupSyncService(
        AppDbContext context,
        IApiFutebolService apiFutebolService)
    {
        _context = context;
        _apiFutebolService = apiFutebolService;
    }

    public async Task<SyncWorldCupResponse> SyncAsync(
        CancellationToken cancellationToken)
    {
        var json = await _apiFutebolService.GetWorldCupGroupStageAsync();

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        var response = new SyncWorldCupResponse
        {
            ChampionshipsSynced = await SyncChampionshipAsync(root, cancellationToken)
        };

        if (!root.TryGetProperty("grupos", out var grupos))
        {
            await _context.SaveChangesAsync(cancellationToken);
            return response;
        }

        foreach (var groupProperty in grupos.EnumerateObject())
        {
            var group = groupProperty.Value;

            var groupName = GetString(group, "nome");
            var groupSlug = GetString(group, "slug");

            if (group.TryGetProperty("times", out var teams))
            {
                foreach (var team in teams.EnumerateArray())
                {
                    var synced = await SyncTeamAsync(team, cancellationToken);

                    if (synced)
                        response.TeamsSynced++;
                }
            }

            if (group.TryGetProperty("partidas", out var rounds))
            {
                foreach (var roundProperty in rounds.EnumerateObject())
                {
                    var roundSlug = roundProperty.Name;
                    var roundNumber = ExtractRoundNumber(roundSlug);
                    var roundName = roundNumber > 0
                        ? $"{roundNumber}ª Rodada"
                        : roundSlug;

                    foreach (var match in roundProperty.Value.EnumerateArray())
                    {
                        var synced = await SyncMatchAsync(
                            match,
                            groupName,
                            groupSlug,
                            roundName,
                            roundSlug,
                            roundNumber,
                            cancellationToken);

                        if (synced)
                            response.MatchesSynced++;
                    }
                }
            }

            if (group.TryGetProperty("tabela", out var standings))
            {
                foreach (var standing in standings.EnumerateArray())
                {
                    var synced = await SyncStandingAsync(
                        standing,
                        groupName,
                        groupSlug,
                        cancellationToken);

                    if (synced)
                        response.StandingsSynced++;
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return response;
    }

    private async Task<int> SyncChampionshipAsync(
        JsonElement root,
        CancellationToken cancellationToken)
    {
        var campeonato = root.GetProperty("campeonato");

        var externalId = GetInt(campeonato, "campeonato_id");

        var entity = _context.Championships.Local
            .FirstOrDefault(x => x.ExternalId == externalId);

        entity ??= await _context.Championships
            .FirstOrDefaultAsync(x => x.ExternalId == externalId, cancellationToken);

        if (entity == null)
        {
            entity = new Championship
            {
                ExternalId = externalId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Championships.Add(entity);
        }

        entity.Name = GetString(campeonato, "nome");
        entity.PopularName = GetString(campeonato, "nome_popular");
        entity.Slug = GetString(campeonato, "slug");
        entity.Status = GetString(campeonato, "status");
        entity.Type = GetString(campeonato, "tipo");
        entity.Region = GetString(campeonato, "regiao");
        entity.LogoUrl = GetString(campeonato, "logo");

        if (campeonato.TryGetProperty("edicao_atual", out var edicaoAtual))
            entity.Season = GetString(edicaoAtual, "temporada");

        entity.UpdatedAt = DateTime.UtcNow;

        return 1;
    }

    private async Task<bool> SyncTeamAsync(
        JsonElement team,
        CancellationToken cancellationToken)
    {
        var externalId = GetInt(team, "time_id");

        if (externalId <= 0)
            return false;

        var entity = _context.FootballTeams.Local
            .FirstOrDefault(x => x.ExternalId == externalId);

        entity ??= await _context.FootballTeams
            .FirstOrDefaultAsync(x => x.ExternalId == externalId, cancellationToken);

        var created = false;

        if (entity == null)
        {
            entity = new FootballTeam
            {
                ExternalId = externalId,
                CreatedAt = DateTime.UtcNow
            };

            _context.FootballTeams.Add(entity);
            created = true;
        }

        entity.Name = GetString(team, "nome_popular");
        entity.Acronym = GetString(team, "sigla");
        entity.LogoUrl = GetString(team, "escudo");
        entity.UpdatedAt = DateTime.UtcNow;

        return created;
    }

    private async Task<bool> SyncMatchAsync(
    JsonElement match,
    string groupName,
    string groupSlug,
    string roundName,
    string roundSlug,
    int roundNumber,
    CancellationToken cancellationToken)
    {
        var externalId = GetInt(match, "partida_id");

        if (externalId <= 0)
            return false;

        var entity = _context.FootballMatches.Local
            .FirstOrDefault(x => x.ExternalId == externalId);

        entity ??= await _context.FootballMatches
            .FirstOrDefaultAsync(x => x.ExternalId == externalId, cancellationToken);

        var created = false;

        if (entity == null)
        {
            entity = new FootballMatch
            {
                ExternalId = externalId,
                CreatedAt = DateTime.UtcNow
            };

            _context.FootballMatches.Add(entity);
            created = true;
        }

        var homeTeam = match.GetProperty("time_mandante");
        var awayTeam = match.GetProperty("time_visitante");

        await SyncTeamAsync(homeTeam, cancellationToken);
        await SyncTeamAsync(awayTeam, cancellationToken);

        var newStatus = GetString(match, "status");
        var newHomeScore = GetNullableInt(match, "placar_mandante");
        var newAwayScore = GetNullableInt(match, "placar_visitante");

        var resultChanged =
            entity.Status != newStatus ||
            entity.HomeScore != newHomeScore ||
            entity.AwayScore != newAwayScore;

        entity.ChampionshipExternalId = WorldCupChampionshipId;
        entity.PhaseExternalId = GroupStagePhaseId;
        entity.GroupName = groupName;
        entity.GroupSlug = groupSlug;
        entity.RoundName = roundName;
        entity.RoundSlug = roundSlug;
        entity.RoundNumber = roundNumber;
        entity.ScoreText = GetString(match, "placar");
        entity.Status = newStatus;
        entity.Slug = GetString(match, "slug");
        entity.HomeTeamExternalId = GetInt(homeTeam, "time_id");
        entity.AwayTeamExternalId = GetInt(awayTeam, "time_id");
        entity.HomeScore = newHomeScore;
        entity.AwayScore = newAwayScore;
        entity.HasPenaltyShootout = GetBool(match, "disputa_penalti");
        entity.MatchDateText = GetString(match, "data_realizacao");
        entity.MatchTimeText = GetString(match, "hora_realizacao");
        entity.MatchDate = ParseApiDate(GetString(match, "data_realizacao_iso"));

        if (resultChanged)
            entity.PointsCalculated = false;

        if (match.TryGetProperty("estadio", out var stadium) &&
            stadium.ValueKind != JsonValueKind.Null)
        {
            entity.StadiumExternalId = GetNullableInt(stadium, "estadio_id");
            entity.StadiumName = GetString(stadium, "nome_popular");
        }
        else
        {
            entity.StadiumExternalId = null;
            entity.StadiumName = string.Empty;
        }

        entity.UpdatedAt = DateTime.UtcNow;

        return created || resultChanged;
    }

    private async Task<bool> SyncStandingAsync(
        JsonElement standing,
        string groupName,
        string groupSlug,
        CancellationToken cancellationToken)
    {
        if (!standing.TryGetProperty("time", out var team))
            return false;

        await SyncTeamAsync(team, cancellationToken);

        var teamExternalId = GetInt(team, "time_id");

        var entity = _context.FootballGroupStandings.Local
            .FirstOrDefault(x =>
                x.ChampionshipExternalId == WorldCupChampionshipId &&
                x.PhaseExternalId == GroupStagePhaseId &&
                x.GroupSlug == groupSlug &&
                x.TeamExternalId == teamExternalId);

        entity ??= await _context.FootballGroupStandings
            .FirstOrDefaultAsync(x =>
                x.ChampionshipExternalId == WorldCupChampionshipId &&
                x.PhaseExternalId == GroupStagePhaseId &&
                x.GroupSlug == groupSlug &&
                x.TeamExternalId == teamExternalId,
                cancellationToken);

        var created = false;

        if (entity == null)
        {
            entity = new FootballGroupStanding
            {
                ChampionshipExternalId = WorldCupChampionshipId,
                PhaseExternalId = GroupStagePhaseId,
                GroupName = groupName,
                GroupSlug = groupSlug,
                TeamExternalId = teamExternalId,
                CreatedAt = DateTime.UtcNow
            };

            _context.FootballGroupStandings.Add(entity);
            created = true;
        }

        entity.GroupName = groupName;
        entity.GroupSlug = groupSlug;
        entity.Position = GetInt(standing, "posicao");
        entity.Points = GetInt(standing, "pontos");
        entity.Games = GetInt(standing, "jogos");
        entity.Wins = GetInt(standing, "vitorias");
        entity.Draws = GetInt(standing, "empates");
        entity.Losses = GetInt(standing, "derrotas");
        entity.GoalsFor = GetInt(standing, "gols_pro");
        entity.GoalsAgainst = GetInt(standing, "gols_contra");
        entity.GoalDifference = GetInt(standing, "saldo_gols");
        entity.Performance = GetDecimal(standing, "aproveitamento");
        entity.PositionVariation = GetInt(standing, "variacao_posicao");
        entity.QualificationZone = GetNullableString(standing, "faixa_classificacao");
        entity.UpdatedAt = DateTime.UtcNow;

        return created;
    }

    private static string GetString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) &&
               property.ValueKind != JsonValueKind.Null
            ? property.GetString() ?? string.Empty
            : string.Empty;
    }

    private static string? GetNullableString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) &&
               property.ValueKind != JsonValueKind.Null
            ? property.GetString()
            : null;
    }

    private static int GetInt(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) &&
               property.ValueKind != JsonValueKind.Null
            ? property.GetInt32()
            : 0;
    }

    private static int? GetNullableInt(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) &&
               property.ValueKind != JsonValueKind.Null
            ? property.GetInt32()
            : null;
    }

    private static bool GetBool(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) &&
               property.ValueKind != JsonValueKind.Null &&
               property.GetBoolean();
    }

    private static decimal GetDecimal(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) &&
               property.ValueKind != JsonValueKind.Null
            ? property.GetDecimal()
            : 0;
    }

    private static int ExtractRoundNumber(string roundSlug)
    {
        var digits = new string(roundSlug.TakeWhile(char.IsDigit).ToArray());

        return int.TryParse(digits, out var result)
            ? result
            : 0;
    }

    private static DateTime? ParseApiDate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (DateTimeOffset.TryParseExact(
                value,
                "yyyy-MM-ddTHH:mm:sszzz",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsed))
        {
            return parsed.UtcDateTime;
        }

        return DateTimeOffset.TryParse(value, out var fallback)
            ? fallback.UtcDateTime
            : null;
    }
}
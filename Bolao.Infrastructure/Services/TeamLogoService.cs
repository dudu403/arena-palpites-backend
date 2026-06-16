using System.Text;
using System.Text.RegularExpressions;
using Bolao.Application.Common.Interfaces;
using Bolao.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bolao.Infrastructure.Services;

public sealed class TeamLogoService : ITeamLogoService
{
    private const int MaxBytes = 300 * 1024;

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/png",
        "image/jpeg",
        "image/jpg",
        "image/webp",
        "image/svg+xml"
    };

    private static readonly Dictionary<int, string> CustomNationalTeamFlags = new()
    {
        { 330, "https://flagcdn.com/ec.svg" }, 
        { 452, "https://flagcdn.com/qa.svg" },
        { 455, "https://flagcdn.com/sa.svg" },
        { 457, "https://flagcdn.com/tn.svg" },
        { 692, "https://flagcdn.com/nz.svg" },
        { 701, "https://flagcdn.com/za.svg" },
        { 1056, "https://flagcdn.com/ba.svg" },
        { 347, "https://flagcdn.com/cz.svg" },
    };

    private readonly AppDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly ILogger<TeamLogoService> _logger;

    public TeamLogoService(
        AppDbContext context,
        HttpClient httpClient,
        ILogger<TeamLogoService> logger)
    {
        _context = context;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<TeamLogoFileResult?> GetLogoAsync(
        int externalTeamId,
        CancellationToken cancellationToken)
    {
        if (externalTeamId <= 0)
            return null;

        var logoUrl = await ResolveLogoUrlAsync(externalTeamId, cancellationToken);

        if (string.IsNullOrWhiteSpace(logoUrl))
            return null;

        if (!Uri.TryCreate(logoUrl, UriKind.Absolute, out var logoUri))
            return null;

        if (logoUri.Scheme != Uri.UriSchemeHttps)
            return null;

        using var request = new HttpRequestMessage(HttpMethod.Get, logoUri);
        request.Headers.UserAgent.ParseAdd("ArenaPalpites/1.0");

        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Falha ao baixar escudo/bandeira. ExternalTeamId: {ExternalTeamId}. StatusCode: {StatusCode}",
                externalTeamId,
                (int)response.StatusCode);

            return null;
        }

        var contentType = response.Content.Headers.ContentType?.MediaType;

        if (string.IsNullOrWhiteSpace(contentType) ||
            !AllowedContentTypes.Contains(contentType))
        {
            _logger.LogWarning(
                "Content-Type inválido para escudo/bandeira. ExternalTeamId: {ExternalTeamId}. ContentType: {ContentType}",
                externalTeamId,
                contentType);

            return null;
        }

        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);

        if (bytes.Length == 0 || bytes.Length > MaxBytes)
        {
            _logger.LogWarning(
                "Tamanho inválido do escudo/bandeira. ExternalTeamId: {ExternalTeamId}. Size: {Size}",
                externalTeamId,
                bytes.Length);

            return null;
        }

        if (contentType.Equals("image/svg+xml", StringComparison.OrdinalIgnoreCase))
        {
            var svg = Encoding.UTF8.GetString(bytes);
            var sanitizedSvg = SanitizeSvg(svg);

            return new TeamLogoFileResult
            {
                Content = Encoding.UTF8.GetBytes(sanitizedSvg),
                ContentType = "image/svg+xml"
            };
        }

        return new TeamLogoFileResult
        {
            Content = bytes,
            ContentType = contentType
        };
    }

    private async Task<string?> ResolveLogoUrlAsync(
        int externalTeamId,
        CancellationToken cancellationToken)
    {
        if (CustomNationalTeamFlags.TryGetValue(externalTeamId, out var customFlagUrl))
            return customFlagUrl;

        var team = await _context.FootballTeams
            .AsNoTracking()
            .Where(x => x.ExternalId == externalTeamId)
            .Select(x => new
            {
                x.LogoUrl
            })
            .FirstOrDefaultAsync(cancellationToken);

        return team?.LogoUrl;
    }

    private static string SanitizeSvg(string svg)
    {
        if (string.IsNullOrWhiteSpace(svg))
            return string.Empty;

        svg = RemoveDangerousSvgContent(svg);

        var stylesByClass = ExtractCssClasses(svg);

        svg = Regex.Replace(
            svg,
            @"<style\b[^>]*>[\s\S]*?</style>",
            string.Empty,
            RegexOptions.IgnoreCase);

        svg = InlineClassStyles(svg, stylesByClass);

        return svg.Trim();
    }

    private static Dictionary<string, string> ExtractCssClasses(string svg)
    {
        var stylesByClass = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var styleMatches = Regex.Matches(
            svg,
            @"<style\b[^>]*>(?<css>[\s\S]*?)</style>",
            RegexOptions.IgnoreCase);

        foreach (Match styleMatch in styleMatches)
        {
            var css = styleMatch.Groups["css"].Value;

            var classMatches = Regex.Matches(
                css,
                @"\.(?<name>[a-zA-Z0-9_-]+)\s*\{(?<style>[^}]*)\}",
                RegexOptions.Singleline);

            foreach (Match classMatch in classMatches)
            {
                var className = classMatch.Groups["name"].Value.Trim();
                var style = classMatch.Groups["style"].Value.Trim();

                if (string.IsNullOrWhiteSpace(className) ||
                    string.IsNullOrWhiteSpace(style))
                    continue;

                stylesByClass[className] = NormalizeCssStyle(style);
            }
        }

        return stylesByClass;
    }

    private static string InlineClassStyles(
        string svg,
        Dictionary<string, string> stylesByClass)
    {
        if (stylesByClass.Count == 0)
            return svg;

        return Regex.Replace(
            svg,
            @"\sclass\s*=\s*""(?<classes>[^""]+)""",
            match =>
            {
                var classes = match.Groups["classes"].Value
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                var inlineStyles = classes
                    .Where(stylesByClass.ContainsKey)
                    .Select(className => stylesByClass[className])
                    .Where(style => !string.IsNullOrWhiteSpace(style))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (inlineStyles.Count == 0)
                    return string.Empty;

                return $@" style=""{string.Join(" ", inlineStyles)}""";
            },
            RegexOptions.IgnoreCase);
    }

    private static string NormalizeCssStyle(string style)
    {
        style = style
            .Replace("\r", string.Empty)
            .Replace("\n", string.Empty)
            .Replace("\t", string.Empty)
            .Trim();

        if (!style.EndsWith(';'))
            style += ";";

        return style;
    }

    private static string RemoveDangerousSvgContent(string svg)
    {
        svg = Regex.Replace(
            svg,
            @"<script\b[^>]*>[\s\S]*?</script>",
            string.Empty,
            RegexOptions.IgnoreCase);

        svg = Regex.Replace(
            svg,
            @"\son[a-zA-Z]+\s*=\s*""[^""]*""",
            string.Empty,
            RegexOptions.IgnoreCase);

        svg = Regex.Replace(
            svg,
            @"\son[a-zA-Z]+\s*=\s*'[^']*'",
            string.Empty,
            RegexOptions.IgnoreCase);

        svg = Regex.Replace(
            svg,
            @"javascript:",
            string.Empty,
            RegexOptions.IgnoreCase);

        return svg;
    }
}
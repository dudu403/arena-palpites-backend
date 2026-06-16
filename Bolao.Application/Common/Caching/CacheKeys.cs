namespace Bolao.Application.Common.Caching;

public static class CacheKeys
{
    private const string HomePrefix = "home";
    private const string RankingPrefix = "ranking";
    private const string DashboardPrefix = "dashboard";
    private const string MatchPrefix = "match";

    public static string Home(string firebaseUid)
        => $"{HomePrefix}:user:{firebaseUid}";

    public static string BolaoRanking(Guid bolaoId)
        => $"{RankingPrefix}:bolao:{bolaoId}";

    public static string BolaoDashboard(
        Guid bolaoId,
        string firebaseUid)
        => $"{DashboardPrefix}:bolao:{bolaoId}:user:{firebaseUid}";

    public static string MatchDetails(
        Guid matchId,
        Guid bolaoId,
        string firebaseUid)
        => $"{MatchPrefix}:{matchId}:bolao:{bolaoId}:user:{firebaseUid}";
}
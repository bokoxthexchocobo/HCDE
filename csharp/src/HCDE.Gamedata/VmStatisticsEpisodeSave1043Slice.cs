namespace HCDE.Gamedata;

/// <summary>
/// Episode save, serialization, stat display, and CCMD/ADD_STAT tail from <c>statistics.cpp</c> (**470–619**).
/// Chunk **1043**. Parser/time helpers: uncredited <see cref="VmStatisticsParser"/> / <see cref="VmStatisticsTimeFormat"/>.
/// </summary>
public static class VmStatisticsEpisodeSave1043Slice
{
    public const int GsLevel = 0;
    public const int GsCutscene = 9;
    public const int SaveStatisticsSingleLevelMode = 2;

    public readonly record struct EpisodeStatTotals(
        int KillCount,
        int TotalKills,
        int ItemCount,
        int TotalItems,
        int SecretCount,
        int TotalSecrets);

    public static EpisodeStatTotals AccumulateEpisodeTotals(IReadOnlyList<VmOneLevelStats> levels)
    {
        var kills = 0;
        var totalKills = 0;
        var items = 0;
        var totalItems = 0;
        var secrets = 0;
        var totalSecrets = 0;
        for (var i = 0; i < levels.Count; i++)
        {
            var level = levels[i];
            kills += level.KillCount;
            totalKills += level.TotalKills;
            items += level.ItemCount;
            totalItems += level.TotalItems;
            secrets += level.SecretCount;
            totalSecrets += level.TotalSecrets;
        }

        return new EpisodeStatTotals(kills, totalKills, items, totalItems, secrets, totalSecrets);
    }

    public static string FormatEpisodeSummaryInfoLine(EpisodeStatTotals totals, int validLevelCount) =>
        string.Format(
            "{0,4}/{1,4}, {2,4}/{3,4}, {4,3}/{5,3}, {6,2}",
            totals.KillCount,
            totals.TotalKills,
            totals.ItemCount,
            totals.TotalItems,
            totals.SecretCount,
            totals.TotalSecrets,
            validLevelCount);

    public static string FormatLevelStatInfoLine(VmOneLevelStats level) =>
        string.Format(
            "{0,4}/{1,4}, {2,4}/{3,4}, {4,3}/{5,3}",
            level.KillCount,
            level.TotalKills,
            level.ItemCount,
            level.TotalItems,
            level.SecretCount,
            level.TotalSecrets);

    public static bool IsSingleLevelStatisticsSaveMode(int saveStatistics) =>
        saveStatistics == SaveStatisticsSingleLevelMode;

    public static int? TryMatchStartEpisodeIndex(
        IReadOnlyList<string> episodeMaps,
        string startLevelName)
    {
        for (var j = 0; j < episodeMaps.Count; j++)
        {
            if (episodeMaps[j].Equals(startLevelName, StringComparison.OrdinalIgnoreCase))
                return j;
        }

        return null;
    }

    public static string ResolveEpisodeNameForDisplay(string? episodeName) =>
        string.IsNullOrEmpty(episodeName) ? string.Empty : episodeName;

    public static string FormatLevelStatDisplayLine(VmOneLevelStats level)
    {
        var minutes = VmStatisticsTimeFormat.Minutes(level.LevelTime);
        var seconds = VmStatisticsTimeFormat.Seconds(level.LevelTime);
        return string.Format(
            "Level {0} - Kills: {1}/{2} - Items: {3}/{4} - Secrets: {5}/{6} - Time: {7}:{8:D2}\n",
            level.LevelName,
            level.KillCount,
            level.TotalKills,
            level.ItemCount,
            level.TotalItems,
            level.SecretCount,
            level.TotalSecrets,
            minutes,
            seconds);
    }

    public static string ComposeLevelStatReport(IReadOnlyList<VmOneLevelStats> levels)
    {
        var compose = new System.Text.StringBuilder();
        for (var i = 0; i < levels.Count; i++)
            compose.Append(FormatLevelStatDisplayLine(levels[i]));
        return compose.ToString();
    }

    public static bool CanUseFinishGameCommand(int gameState) =>
        gameState == GsLevel || gameState == GsCutscene;

    public static string FormatFinishGameBlockedMessage() =>
        "Cannot use 'finishgame' while not in a game!\n";

    public static bool ShouldIncludeVelocityStat(int gameState, object? cameraActor) =>
        cameraActor != null && gameState == GsLevel;

    public static object? ResolveVelocityStatCamera(object? camera, bool cameraHasPlayer, object? playerMob) =>
        camera is null || !cameraHasPlayer ? playerMob : camera;

    public static string FormatVelocityStatBlock(double currentVelocity, double maxVelocity, double avgVelocity, string mapName) =>
        string.Format(
            "Current velocity: {0:F2}\nLevel {1} - Velocity Max: {2:F2}, Velocity Average: {3:F2}\n",
            currentVelocity,
            mapName,
            maxVelocity,
            avgVelocity);
}

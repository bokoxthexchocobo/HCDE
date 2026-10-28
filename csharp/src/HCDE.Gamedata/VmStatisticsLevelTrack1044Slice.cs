namespace HCDE.Gamedata;

/// <summary>
/// <c>LevelStatEntry</c>, <c>STAT_StartNewGame</c>, <c>StoreLevelStats</c>, and <c>STAT_ChangeLevel</c> head from <c>statistics.cpp</c> (**320–469**).
/// Chunk **1044**; follows **1043** (**470–619**). Episode save tail / aggregation: <see cref="VmStatisticsEpisodeSave1043Slice"/>.
/// </summary>
public static class VmStatisticsLevelTrack1044Slice
{
    public const int GsLevel = 0;
    public const int Level2NoStatistics = unchecked((int)0x10000000);
    public const int Level2EndGame = unchecked((int)0x20000000);
    public const int SaveStatisticsEpisodeMode = 1;
    public const int LevelStatNameMaxLen = 23;
    public const int LevelStatInfoMaxLen = 59;
    public const string EndSequenceMapPrefix = "enDSeQ";

    public readonly record struct LevelStatRecord(string Name, string Info, int TimeNeeded);

    public readonly record struct LevelCounterSnapshot(
        int TotalMonsters,
        int KilledMonsters,
        int TotalItems,
        int FoundItems,
        int TotalSecrets,
        int FoundSecrets,
        int MapTime);

    public static LevelStatRecord BuildLevelStatEntry(string levelName, string infoText, int playtime) =>
        new(
            TruncateFixedField(levelName, LevelStatNameMaxLen),
            TruncateFixedField(infoText, LevelStatInfoMaxLen),
            playtime);

    public static bool ShouldAssignStartEpisodeOnNewGame(bool deathmatch, bool multiplayer) =>
        !deathmatch && !multiplayer;

    public static int? TryResolveStartEpisodeIndex(IReadOnlyList<string> episodeMaps, string mapName) =>
        VmStatisticsEpisodeSave1043Slice.TryMatchStartEpisodeIndex(episodeMaps, mapName);

    public static bool ShouldStoreLevelStatistics(int gameState) => gameState == GsLevel;

    public static bool ShouldSkipLevelStatistics(int levelFlags2) => (levelFlags2 & Level2NoStatistics) != 0;

    public static int FindOrReserveLevelIndex(IList<VmOneLevelStats> levelData, string mapName)
    {
        for (var i = 0; i < levelData.Count; i++)
        {
            if (levelData[i].LevelName.Equals(mapName, StringComparison.OrdinalIgnoreCase))
                return i;
        }

        levelData.Add(new VmOneLevelStats { LevelName = mapName });
        return levelData.Count - 1;
    }

    public static void ApplyLevelCounterSnapshot(VmOneLevelStats entry, LevelCounterSnapshot snapshot)
    {
        entry.TotalKills = snapshot.TotalMonsters;
        entry.KillCount = snapshot.KilledMonsters;
        entry.TotalItems = snapshot.TotalItems;
        entry.ItemCount = snapshot.FoundItems;
        entry.TotalSecrets = snapshot.TotalSecrets;
        entry.SecretCount = snapshot.FoundSecrets;
        entry.LevelTime = snapshot.MapTime;
    }

    public static int ReconcileKillCountWhenNoLivingMonsters(
        int killCount,
        int totalKills,
        int livingCountKillMonsters) =>
        livingCountKillMonsters == 0 ? totalKills : killCount;

    public static int CompareLevelNamesNatural(string? left, string? right)
    {
        if (left is null && right is null) return 0;
        if (left is null) return -1;
        if (right is null) return 1;
        return string.Compare(left, right, StringComparison.OrdinalIgnoreCase);
    }

    public static void SortLevelDataByName(List<VmOneLevelStats> levelData) =>
        levelData.Sort((a, b) => CompareLevelNamesNatural(a.LevelName, b.LevelName));

    public static bool IsEndSequenceMapTransition(string newLevelName) =>
        newLevelName.StartsWith(EndSequenceMapPrefix, StringComparison.Ordinal);

    public static bool ShouldResolveNextLevelInfo(string newLevelName) =>
        !IsEndSequenceMapTransition(newLevelName);

    public static bool ShouldPersistEpisodeStatistics(int saveStatistics) =>
        saveStatistics == SaveStatisticsEpisodeMode;

    public static bool ReachedEpisodeStatisticsCheckpoint(
        bool nextLevelInfoMissing,
        int nextLevelFlags2,
        bool startEpisodeActive) =>
        (nextLevelInfoMissing || (nextLevelFlags2 & Level2EndGame) != 0) && startEpisodeActive;

    public static string BuildEpisodeStatisticsSectionKey(string wadBaseName, string episodeMap) =>
        (wadBaseName + "." + episodeMap).ToUpperInvariant();

    public static string ResolveEpisodeNameForStatistics(string episodeName, Func<string, string>? localizedLookup)
    {
        if (string.IsNullOrEmpty(episodeName))
            return string.Empty;
        if (episodeName[0] != '$')
            return episodeName;
        var key = episodeName[1..];
        return localizedLookup?.Invoke(key) ?? key;
    }

    private static string TruncateFixedField(string value, int maxLen) =>
        value.Length <= maxLen ? value : value[..maxLen];
}

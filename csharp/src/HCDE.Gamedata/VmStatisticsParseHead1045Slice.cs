namespace HCDE.Gamedata;

/// <summary>
/// Statistics globals layout and <c>ParseStatistics</c> head from <c>statistics.cpp</c> (**1–150**).
/// Chunk **1045**; file tail **320–619** credited **1044–1043**. Token document parser: uncredited <see cref="VmStatisticsParser"/>.
/// </summary>
public static class VmStatisticsParseHead1045Slice
{
    public const int StatInfoFieldLen = 60;
    public const int StatNameFieldLen = 24;
    public const int OneLevelCounterFieldCount = 6;

    public readonly record struct StatisticsEpisodeDraft(string Header, string DisplayName);

    public readonly record struct StatisticsSessionDraft(
        string Name,
        string Info,
        int TimeNeededTics,
        int Skill);

    public readonly record struct StatisticsLevelNameDraft(string Name);

    public static IReadOnlyList<string> OneLevelCounterNames { get; } =
    [
        "totalkills",
        "killcount",
        "totalitems",
        "itemcount",
        "totalsecrets",
        "secretcount",
    ];

    public static bool ShouldSkipParseWhenFileUnavailable(bool fileOpened) => !fileOpened;

    public static bool ShouldConsumeAnotherEpisodeEntry(bool scannerReturnedString) => scannerReturnedString;

    public static bool ShouldParseAnotherSessionEntry(bool episodeSectionCloseMatched) =>
        !episodeSectionCloseMatched;

    public static bool ShouldParseAnotherNestedLevelEntry(bool levelSectionCloseMatched) =>
        !levelSectionCloseMatched;

    public static StatisticsEpisodeDraft BuildEpisodeDraft(string headerToken, string nameToken) =>
        new(headerToken, nameToken);

    public static bool IsStatisticsBlockOpenToken(string token) =>
        token.Equals("{", StringComparison.Ordinal);

    public static string SelectSessionNameFromFileTokens(string firstToken, string secondToken) =>
        TruncateStatField(secondToken, StatNameFieldLen);

    public static string TruncateStatInfoField(string value) =>
        TruncateStatField(value, StatInfoFieldLen);

    public static int ParseColonDurationToTics(string colonTime) =>
        VmStatisticsParser.ParseTimeToTics(colonTime);

    public static StatisticsSessionDraft BuildSessionDraft(
        string firstToken,
        string secondToken,
        string infoToken,
        string durationToken,
        int skill)
    {
        var name = SelectSessionNameFromFileTokens(firstToken, secondToken);
        var info = TruncateStatInfoField(infoToken);
        var tics = ParseColonDurationToTics(durationToken);
        return new StatisticsSessionDraft(name, info, tics, skill);
    }

    public static bool TryBuildSessionDraft(
        string firstToken,
        string secondToken,
        string infoToken,
        string durationToken,
        int skill,
        out StatisticsSessionDraft draft)
    {
        draft = default;
        if (string.IsNullOrEmpty(secondToken) || string.IsNullOrEmpty(infoToken))
            return false;
        if (!durationToken.Contains(':', StringComparison.Ordinal))
            return false;
        draft = BuildSessionDraft(firstToken, secondToken, infoToken, durationToken, skill);
        return true;
    }

    public static bool ShouldBeginSessionLevelBlock(string token) =>
        IsStatisticsBlockOpenToken(token);

    public static StatisticsLevelNameDraft BeginNestedLevelStatEntry(string levelNameToken) =>
        new(ParseLevelStatNameFromToken(levelNameToken));

    public static string ParseLevelStatNameFromToken(string levelNameToken) =>
        TruncateStatField(levelNameToken, StatNameFieldLen);

    public static string TakePendingLevelInfoToken(string infoToken) =>
        TruncateStatInfoField(infoToken);

    public static string DefaultStatFileName() => VmStatisticsConfig.StatFileDefault;

    public static int DefaultSaveStatisticsMode() => VmStatisticsConfig.SaveStatisticsDefault;

    private static string TruncateStatField(string value, int maxLen) =>
        value.Length <= maxLen ? value : value[..maxLen];
}

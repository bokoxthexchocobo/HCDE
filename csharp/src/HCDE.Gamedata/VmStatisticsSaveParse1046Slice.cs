namespace HCDE.Gamedata;

/// <summary>
/// <c>ParseStatistics</c> tail, <c>ReadStatistics</c>, save/sort helpers, and <c>StatisticsEntry</c> from <c>statistics.cpp</c> (**151–319**).
/// Chunk **1046**; completes file with **1045** (**1–150**) and **1044–1043** (**320–619**).
/// </summary>
public static class VmStatisticsSaveParse1046Slice
{
    public const string FallbackStatisticsEntryDate = "00.00.0000";

    public readonly record struct StatisticsNestedLevelDraft(string Name, string Info, int TimeNeededTics)
    {
        public int Skill => 0;
    }

    public readonly record struct StatisticsEntryDraft(string DateName, string Info, int TimeNeededTics, int Skill);

    public static StatisticsNestedLevelDraft BuildNestedLevelStatDraft(
        string levelNameToken,
        string infoToken,
        string durationToken) =>
        new(
            VmStatisticsParseHead1045Slice.ParseLevelStatNameFromToken(levelNameToken),
            VmStatisticsParseHead1045Slice.TakePendingLevelInfoToken(infoToken),
            VmStatisticsParseHead1045Slice.ParseColonDurationToTics(durationToken));

    public static bool ShouldReloadEpisodeStatistics(string statFileName) =>
        !string.IsNullOrWhiteSpace(statFileName);

    public static int CompareEpisodeHeaderNamesNatural(string? left, string? right) =>
        string.Compare(left, right, StringComparison.OrdinalIgnoreCase);

    public static int ComputeSessionDateSortKey(string sessionDateName)
    {
        var parts = sessionDateName.Split('.');
        if (parts.Length < 3)
            return 0;
        if (!int.TryParse(parts[0], out var day))
            return 0;
        if (!int.TryParse(parts[1], out var month))
            return 0;
        if (!int.TryParse(parts[2], out var year))
            return 0;
        return day + (100 * month) + (2000 * year);
    }

    public static int CompareSessionDateSortKeys(int leftKey, int rightKey) => leftKey.CompareTo(rightKey);

    public static bool ShouldEmitSavedSessionRow(string info) =>
        !string.IsNullOrEmpty(info) && info[0] > '\0';

    public static string FormatSavedEpisodeBlockHeader(string episodeHeader, string episodeName) =>
        $"{episodeHeader} \"{episodeName}\"\n{{\n";

    public static string FormatSavedEpisodeBlockFooter() => "}\n\n";

    public static string FormatSavedSessionRow(
        int oneBasedIndex,
        string sessionDateName,
        string info,
        int timeNeededTics,
        int skill) =>
        string.Format(
            "\t{0,2}. {1,10} \"{2,-33}\" {3:00}:{4:00}:{5:00} {6}\n",
            oneBasedIndex,
            sessionDateName,
            info,
            VmStatisticsTimeFormat.Hours(timeNeededTics),
            VmStatisticsTimeFormat.Minutes(timeNeededTics),
            VmStatisticsTimeFormat.Seconds(timeNeededTics),
            skill);

    public static string FormatSavedNestedLevelRow(string levelName, string info, int timeNeededTics) =>
        string.Format(
            "\t\t{0,-8} \"{1,-33}\" {2:00}:{3:00}:{4:00}\n",
            levelName,
            info,
            VmStatisticsTimeFormat.Hours(timeNeededTics),
            VmStatisticsTimeFormat.Minutes(timeNeededTics),
            VmStatisticsTimeFormat.Seconds(timeNeededTics));

    public static string FormatSavedNestedLevelBlockOpen() => "\t{\n";

    public static string FormatSavedNestedLevelBlockClose() => "\t}\n";

    public static int FindEpisodeStatisticsIndex(IReadOnlyList<string> episodeHeaders, string sectionName)
    {
        for (var i = 0; i < episodeHeaders.Count; i++)
        {
            if (episodeHeaders[i].Equals(sectionName, StringComparison.OrdinalIgnoreCase))
                return i;
        }

        return -1;
    }

    public static string FormatStatisticsEntryDate(DateTime? localTime) =>
        localTime.HasValue
            ? $"{localTime.Value.Day:00}.{localTime.Value.Month:00}.{localTime.Value.Year:0000}"
            : FallbackStatisticsEntryDate;

    public static StatisticsEntryDraft BuildStatisticsEntryDraft(
        string infoText,
        int playtimeTics,
        int skill,
        DateTime? localTime) =>
        new(
            FormatStatisticsEntryDate(localTime),
            infoText,
            playtimeTics,
            skill);

    public static string FormatUnableToSaveStatisticsMessage(string fileName) =>
        $"Unable to save statistics to {fileName}";
}

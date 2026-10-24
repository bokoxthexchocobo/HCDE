namespace HCDE.Client;

/// <summary><c>CheckEpisodeCmd</c> from <c>d_main.cpp</c> (2846–2891).</summary>
public static class VmDMainCheckEpisodeCmd
{
    public static bool TryParseEpisodeArg(string? value, int episodeCount, out int episodeIndex, out bool valid)
    {
        episodeIndex = 0;
        valid = false;
        if (value is null)
            return false;
        episodeIndex = int.Parse(value) - 1;
        if (episodeIndex < 0 || episodeIndex >= episodeCount)
        {
            episodeIndex = 0;
            return true;
        }

        valid = true;
        return true;
    }

    public static bool ShouldKeepExistingStartMap(bool setEpisode, bool setmap) => !setEpisode && setmap;

    public static bool ShouldReplaceLauncherDefault(
        bool setmapFromLauncherDefault,
        int episodeCount,
        string episodeStart,
        string startMap) =>
        setmapFromLauncherDefault
        && episodeCount > 0
        && !string.IsNullOrEmpty(episodeStart)
        && !episodeStart.Equals(startMap, StringComparison.OrdinalIgnoreCase);

    public static string FormatEpisodeRemapMessage(string startMap, string episodeStart) =>
        $"HCDE: startup map {startMap} came from launcher default; using episode start map {episodeStart}.\n";

    public static bool ShouldSetAutostartFromEpisode(bool setEpisode) => setEpisode;
}

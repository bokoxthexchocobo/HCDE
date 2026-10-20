namespace HCDE.Gamedata;

/// <summary><c>ParseEpisodeInfo</c> from <c>g_mapinfo.cpp</c> (2421–2570).</summary>
public sealed class VmMapInfoEpisodeDraft
{
    public string Map { get; set; } = "";
    public string Name { get; set; } = "";
    public string Pic { get; set; } = "";
    public char Key { get; set; }
    public bool Remove { get; set; }
    public bool NoSkillMenu { get; set; }
    public bool Optional { get; set; }
    public bool Extended { get; set; }
}

public static class VmMapInfoEpisodeParser
{
    public static bool ShouldIgnoreExtendedEpisode(bool extended, bool menuHackExtended) =>
        extended && !menuHackExtended;

    public static bool ShouldSkipOptionalEpisode(bool optional, bool mapExists) =>
        optional && !mapExists;

    public static char NormalizeEpisodeKey(string token) =>
        token.Length > 0 ? char.ToLowerInvariant(token[0]) : '\0';

    public static string SynthesizePicName(string picToken) => $"${picToken}";

    public static int FindEpisodeIndex(IReadOnlyList<string> episodeMaps, string map)
    {
        for (var i = 0; i < episodeMaps.Count; i++)
        {
            if (episodeMaps[i].Equals(map, StringComparison.OrdinalIgnoreCase))
                return i;
        }

        return -1;
    }

    public static void ApplyEpisodeDraft(VmMapInfoEpisodeDraft draft, IList<string> episodeMaps, IList<VmMapInfoEpisodeDraft> episodes)
    {
        if (draft.Remove)
        {
            var idx = FindEpisodeIndex(episodeMaps, draft.Map);
            if (idx >= 0)
            {
                episodeMaps.RemoveAt(idx);
                episodes.RemoveAt(idx);
            }

            return;
        }

        var existing = FindEpisodeIndex(episodeMaps, draft.Map);
        if (existing < 0)
        {
            episodeMaps.Add(draft.Map);
            episodes.Add(draft);
        }
        else
        {
            episodes[existing] = draft;
        }
    }
}

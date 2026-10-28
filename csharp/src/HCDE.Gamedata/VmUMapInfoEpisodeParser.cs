namespace HCDE.Gamedata;

public sealed class VmUMapEpisode
{
    public string PicName { get; set; } = "";
    public string EpisodeName { get; set; } = "";
    public char Shortcut { get; set; }
    public string EpisodeMap { get; set; } = "";
}

/// <summary><c>episode</c> property handling from <c>umapinfo.cpp</c>.</summary>
public static class VmUMapInfoEpisodeParser
{
    public static bool TryApplyEpisode(
        string episodeText,
        string mapName,
        IList<VmUMapEpisode> episodes,
        ref int id24LevelNum)
    {
        if (episodeText == "-")
        {
            episodes.Clear();
            return true;
        }

        var split = episodeText.Split('\n');
        var epi = new VmUMapEpisode { EpisodeMap = mapName, PicName = split[0] };
        if (split.Length > 1) epi.EpisodeName = split[1];
        if (split.Length > 2 && split[2].Length > 0) epi.Shortcut = char.ToLowerInvariant(split[2][0]);
        id24LevelNum = 1;

        for (var i = 0; i < episodes.Count; i++)
        {
            if (episodes[i].EpisodeMap.Equals(mapName, StringComparison.OrdinalIgnoreCase))
            {
                episodes[i] = epi;
                return true;
            }
        }

        episodes.Add(epi);
        return true;
    }
}

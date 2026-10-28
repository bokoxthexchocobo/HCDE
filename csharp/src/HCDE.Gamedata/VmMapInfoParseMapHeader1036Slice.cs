namespace HCDE.Gamedata;

/// <summary>
/// <c>ParseMapHeader</c> tail and <c>ParseEpisodeInfo</c> head from <c>g_mapinfo.cpp</c> (**2427–2576**).
/// Chunk **1036**; follows **1035** (**2277–2426**).
/// </summary>
public static class VmMapInfoParseMapHeader1036Slice
{
    public const uint GiShareware = 1u << 0;
    public const uint GiMenuhackExtended = 1u << 1;

    public enum EpisodeBodyKeyword
    {
        None,
        Optional,
        Extended,
        Name,
        PicName,
        Remove,
        Key,
        NoSkillMenu,
        Intro,
        Unknown
    }

    public static void AssignLevelNumFromMapName(VmMapInfoHeaderDraft draft)
    {
        draft.LevelNum = VmMapInfoParseMapHeader1035Slice.GetDefaultLevelNum(draft.MapName, out var id24);
        draft.Id24LevelNum = id24;
    }

    public static bool TryAssignHexenMapMusic(
        IReadOnlyDictionary<int, string> hexenMusicByLevel,
        int levelNum,
        out string music)
    {
        if (hexenMusicByLevel.TryGetValue(levelNum, out music!))
            return true;
        music = "";
        return false;
    }

    public static void ApplySharewareTeaserMap(ref string map, bool isShareware, string teaserMapToken)
    {
        if (isShareware)
            map = teaserMapToken;
    }

    public static EpisodeBodyKeyword ClassifyEpisodeBodyKeyword(string token)
    {
        if (token.Equals("optional", StringComparison.OrdinalIgnoreCase))
            return EpisodeBodyKeyword.Optional;
        if (token.Equals("extended", StringComparison.OrdinalIgnoreCase))
            return EpisodeBodyKeyword.Extended;
        if (token.Equals("name", StringComparison.OrdinalIgnoreCase))
            return EpisodeBodyKeyword.Name;
        if (token.Equals("picname", StringComparison.OrdinalIgnoreCase))
            return EpisodeBodyKeyword.PicName;
        if (token.Equals("remove", StringComparison.OrdinalIgnoreCase))
            return EpisodeBodyKeyword.Remove;
        if (token.Equals("key", StringComparison.OrdinalIgnoreCase))
            return EpisodeBodyKeyword.Key;
        if (token.Equals("noskillmenu", StringComparison.OrdinalIgnoreCase))
            return EpisodeBodyKeyword.NoSkillMenu;
        if (token.Equals("intro", StringComparison.OrdinalIgnoreCase))
            return EpisodeBodyKeyword.Intro;
        return EpisodeBodyKeyword.Unknown;
    }

    public static bool ShouldIgnoreExtendedEpisode(bool extended, uint gameInfoFlags) =>
        extended && (gameInfoFlags & GiMenuhackExtended) == 0;

    public static bool ShouldSkipOptionalEpisode(bool optional, bool remove, bool mapDataExists) =>
        optional && !remove && !mapDataExists;

    public static char NormalizeEpisodeShortcut(string keyToken) =>
        keyToken.Length > 0 ? char.ToLowerInvariant(keyToken[0]) : '\0';

    public static string SynthesizeEpisodeNameFromPic(string picToken) => $"${picToken}";

    public static void ApplyEpisodePicName(ref string name, string picToken, bool nameIsEmpty)
    {
        if (nameIsEmpty)
            name = SynthesizeEpisodeNameFromPic(picToken);
    }

    public static int FindEpisodeMapIndex(IList<string> episodeMaps, string map)
    {
        for (var i = 0; i < episodeMaps.Count; i++)
        {
            if (episodeMaps[i].Equals(map, StringComparison.OrdinalIgnoreCase))
                return i;
        }

        return -1;
    }

    public static bool ShouldRemoveEpisodeEntry(bool remove) => remove;

    public static bool ShouldReserveNewEpisodeSlot(int index, int episodeCount) =>
        index >= episodeCount;

    public static bool ShouldBreakEpisodeBodyLoop(bool closeBraceParsed) => closeBraceParsed;

    public static bool ShouldEmitUnknownEpisodeProperty(bool closeBraceParsed) => !closeBraceParsed;

    public static void ApplyEpisodeRemoveAt(IList<string> episodeMaps, int index)
    {
        if (index >= 0 && index < episodeMaps.Count)
            episodeMaps.RemoveAt(index);
    }

    public static void MergeEpisodeDraft(
        IList<string> episodeMaps,
        IList<VmMapInfoEpisodeDraft> episodes,
        VmMapInfoEpisodeDraft draft)
    {
        var index = FindEpisodeMapIndex(episodeMaps, draft.Map);
        if (index < 0)
        {
            episodeMaps.Add(draft.Map);
            episodes.Add(draft);
            return;
        }

        episodes[index] = draft;
    }
}

namespace HCDE.Gamedata;

/// <summary>
/// CMPGNINF apply tail and <c>G_ParseMapInfo</c> COMPLVL / lump-scan head from <c>g_mapinfo.cpp</c> (**2900–3049**).
/// Chunk **1031**; follows **1030** (**2750–2899**). COMPLVL presets and lump precedence delegate to off-tally
/// <see cref="VmMapInfoComplvlParser"/> / <see cref="VmMapInfoLumpPrecedence"/> (zero duplicate Loop credit **3067–3114**).
/// </summary>
public static class VmMapInfoParseMapInfo1031Slice
{
    public const int GameDoom = 1;

    public sealed class CmpgninfLevelDraft
    {
        public int InvasionWaveLimit { get; set; }
        public int DuelLimit { get; set; }
    }

    public sealed class GameDefaultsCompatDraft
    {
        public uint CompatFlags { get; set; }
        public uint CompatMask { get; set; }
        public uint CompatFlags2 { get; set; }
        public uint CompatMask2 { get; set; }
    }

    public static bool ShouldSkipCmpgninfBlock(VmCmpgninfBlock block) =>
        string.IsNullOrEmpty(block.MapName) || (!block.HasWaveLimit && !block.HasDuelLimit);

    public static void ApplyCmpgninfLimits(VmCmpgninfBlock block, CmpgninfLevelDraft level)
    {
        if (block.HasWaveLimit)
            level.InvasionWaveLimit = block.WaveLimit;
        if (block.HasDuelLimit)
            level.DuelLimit = block.DuelLimit;
    }

    public static bool TryApplyCmpgninfBlockProperty(
        VmCmpgninfBlock block,
        string property,
        string stringValue,
        int numericValue,
        out string? error)
    {
        error = null;
        if (property.Equals("wavelimit", StringComparison.OrdinalIgnoreCase))
        {
            block.WaveLimit = VmCmpgninfParser.ClampWaveLimit(numericValue);
            block.HasWaveLimit = true;
            return true;
        }

        if (property.Equals("duellimit", StringComparison.OrdinalIgnoreCase))
        {
            block.DuelLimit = VmCmpgninfParser.ClampDuelLimit(numericValue);
            block.HasDuelLimit = true;
            return true;
        }

        return VmCmpgninfParser.TryApplyProperty(block, property, stringValue, out error);
    }

    public static bool NeedsSyntheticLevelForCmpgninf(bool levelAlreadyExists) => !levelAlreadyExists;

    public static bool ShouldReadComplvlPresets(int gameType) => gameType == GameDoom;

    public static bool TryLoadComplvlPresets(ReadOnlySpan<char> lumpData, out VmMapInfoComplvlFlags flags) =>
        VmMapInfoComplvlParser.TryParseComplvl(lumpData, out flags);

    public static void MergeComplvlIntoDefaults(GameDefaultsCompatDraft defaults, VmMapInfoComplvlFlags flags)
    {
        defaults.CompatFlags |= flags.Flags1;
        defaults.CompatMask |= flags.Flags1;
        defaults.CompatFlags2 |= flags.Flags2;
        defaults.CompatMask2 |= flags.Flags2;
    }

    public static bool ShouldFatalOnBasemapinfoAddonOverride(int baselumpFileContainer) =>
        baselumpFileContainer > 0;

    public static bool ShouldSkipNamedMapinfoLump(
        int nameIndex,
        bool sameWadHasZmapinfo,
        bool sameWadHasMapinfo,
        bool sameWadHasUmapinfo) =>
        VmMapInfoLumpPrecedence.ShouldSkipLump(
            nameIndex, sameWadHasZmapinfo, sameWadHasMapinfo, sameWadHasUmapinfo);

    public static bool ShouldProbeZmapinfoWhenClassicMapinfo(int nameIndex) =>
        nameIndex == VmMapInfoLumpPrecedence.LumpIndexMapinfo;

    public static bool ShouldProbeNativeOverridesForUmapinfo(int nameIndex) =>
        nameIndex == VmMapInfoLumpPrecedence.LumpIndexUmapinfo;

    public static bool ShouldProbeAllNativeForEmapinfo(int nameIndex) =>
        nameIndex == VmMapInfoLumpPrecedence.LumpIndexEmapinfo;

    /// <summary>Matches native search order when deciding if EMAPINFO loses to another lump in the same WAD.</summary>
    public static bool SameWadHasNativeMapinfoVariant(bool hasZmapinfo, bool hasMapinfo, bool hasUmapinfo) =>
        hasZmapinfo || hasMapinfo || hasUmapinfo;
}

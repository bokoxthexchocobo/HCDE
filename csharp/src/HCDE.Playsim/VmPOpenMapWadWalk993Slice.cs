namespace HCDE.Playsim;

/// <summary>
/// `P_OpenMapData` WAD lump walk, TEXTMAP supplements, and embedded map pick from <c>p_openmap.cpp</c> (**151–300**).
/// </summary>
public static class VmPOpenMapWadWalk993Slice
{
    public const int MapLumpBehavior = 11;
    public const int MapLumpReject = 9;
    public const int MapLumpBlockmap = 10;
    public const int MapLumpGlZNodes = 6;
    public const int MapLumpConversation = 12;
    public const int MapLumpLightmap = 13;
    public const int MapLumpTextmap = 1;

    public const uint IwadMagic = 0x44415749u;
    public const uint PwadMagic = 0x44415750u;

    public enum TextMapSupplementLumpKind
    {
        Unrecognized,
        GlZNodes,
        Blockmap,
        Reject,
        Dialogue,
        Behavior,
        Lightmap,
        EndMap,
    }

    public enum ClassicMapLumpAdvanceResult
    {
        Assigned,
        AbortOpen,
        EndWalk,
    }

    public static bool NextLumpIsClassicDoomFormat(string nextLumpFullName) =>
        !string.Equals(nextLumpFullName, "TEXTMAP", StringComparison.OrdinalIgnoreCase);

    public static bool ShouldAbortAfterMapIndex(int mapIndex) =>
        mapIndex == VmPOpenMapHead992Slice.RequiredMissingIndex;

    public static bool ShouldEndMapLumpWalk(int mapIndex) =>
        mapIndex < 0;

    public static bool ShouldMarkMapHasBehavior(int mapIndex) =>
        mapIndex == MapLumpBehavior;

    public static ClassicMapLumpAdvanceResult TryAdvanceClassicMapLump(
        string lumpName,
        ref int lastIndex,
        bool justCheck,
        out int assignedIndex)
    {
        var scan = VmPOpenMapHead992Slice.ScanMapLumpIndex(
            lumpName,
            lastIndex,
            needRequired: !justCheck,
            out assignedIndex);

        switch (scan)
        {
            case VmPOpenMapHead992Slice.MapLumpScanResult.RequiredMissing:
                return ClassicMapLumpAdvanceResult.AbortOpen;
            case VmPOpenMapHead992Slice.MapLumpScanResult.EndOfMap:
                return ClassicMapLumpAdvanceResult.EndWalk;
            default:
                lastIndex = assignedIndex;
                return ClassicMapLumpAdvanceResult.Assigned;
        }
    }

    public static TextMapSupplementLumpKind ClassifyTextMapSupplementLump(string? lumpName)
    {
        if (lumpName is null)
            return TextMapSupplementLumpKind.Unrecognized;

        if (string.Equals(lumpName, "ZNODES", StringComparison.OrdinalIgnoreCase))
            return TextMapSupplementLumpKind.GlZNodes;

        if (string.Equals(lumpName, "BLOCKMAP", StringComparison.OrdinalIgnoreCase))
            return TextMapSupplementLumpKind.Blockmap;

        if (string.Equals(lumpName, "REJECT", StringComparison.OrdinalIgnoreCase))
            return TextMapSupplementLumpKind.Reject;

        if (string.Equals(lumpName, "DIALOGUE", StringComparison.OrdinalIgnoreCase))
            return TextMapSupplementLumpKind.Dialogue;

        if (string.Equals(lumpName, "BEHAVIOR", StringComparison.OrdinalIgnoreCase))
            return TextMapSupplementLumpKind.Behavior;

        if (string.Equals(lumpName, "LIGHTMAP", StringComparison.OrdinalIgnoreCase))
            return TextMapSupplementLumpKind.Lightmap;

        if (string.Equals(lumpName, "ENDMAP", StringComparison.OrdinalIgnoreCase))
            return TextMapSupplementLumpKind.EndMap;

        return TextMapSupplementLumpKind.Unrecognized;
    }

    public static bool TryResolveTextMapSupplementLump(
        string? lumpName,
        out int mapLumpIndex,
        out bool endTextMap,
        out bool marksBehavior)
    {
        mapLumpIndex = -1;
        endTextMap = false;
        marksBehavior = false;

        switch (ClassifyTextMapSupplementLump(lumpName))
        {
            case TextMapSupplementLumpKind.GlZNodes:
                mapLumpIndex = MapLumpGlZNodes;
                return true;
            case TextMapSupplementLumpKind.Blockmap:
                mapLumpIndex = MapLumpBlockmap;
                return true;
            case TextMapSupplementLumpKind.Reject:
                mapLumpIndex = MapLumpReject;
                return true;
            case TextMapSupplementLumpKind.Dialogue:
                mapLumpIndex = MapLumpConversation;
                return true;
            case TextMapSupplementLumpKind.Behavior:
                mapLumpIndex = MapLumpBehavior;
                marksBehavior = true;
                return true;
            case TextMapSupplementLumpKind.Lightmap:
                mapLumpIndex = MapLumpLightmap;
                return true;
            case TextMapSupplementLumpKind.EndMap:
                endTextMap = true;
                return false;
            default:
                return false;
        }
    }

    public static bool TrySelectEmbeddedMapsWadLump(int lumpWad, int lumpMap, out int selectedLump)
    {
        if (lumpMap > lumpWad)
            lumpWad = lumpMap;

        if (lumpWad == -1)
        {
            selectedLump = -1;
            return false;
        }

        selectedLump = lumpWad;
        return true;
    }

    public static bool IsRecognizedWadMagic(uint containerId) =>
        containerId == IwadMagic || containerId == PwadMagic;

    public static bool ResourceEntryIsTextMapHeader(string lumpName, int entryIndex) =>
        entryIndex == 1
        && lumpName.AsSpan(0, Math.Min(lumpName.Length, VmPOpenMapHead992Slice.MapLumpNameCompareLength))
            .Equals("TEXTMAP", StringComparison.OrdinalIgnoreCase);
}

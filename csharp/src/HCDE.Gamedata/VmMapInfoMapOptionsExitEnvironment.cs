namespace HCDE.Gamedata;

public sealed class VmMapInfoExitMapTextEntry
{
    public string Text { get; set; } = "";
    public bool LookupText { get; set; }
    public bool HasText { get; set; }
    public string Music { get; set; } = "";
    public int MusicOrder { get; set; }
    public bool HasMusic { get; set; }
    public string Backdrop { get; set; } = "";
    public bool BackdropIsPic { get; set; }
    public bool HasBackdrop { get; set; }
}

/// <summary>MAPINFO <c>mapbackground</c> through <c>hazardflash</c> (<c>g_mapinfo.cpp</c> 1567–1652).</summary>
public static class VmMapInfoMapOptionsExitEnvironment
{
    public static void ApplyMapBackground(VmMapInfoLevelInfo info, string lump) => info.MapBackground = lump;

    public static void MergeExitText(
        Dictionary<string, VmMapInfoExitMapTextEntry> table,
        string forMap,
        string text,
        bool lookup)
    {
        if (!table.TryGetValue(forMap, out var entry))
        {
            entry = new VmMapInfoExitMapTextEntry();
            table[forMap] = entry;
        }

        entry.Text = text;
        entry.LookupText = lookup;
        entry.HasText = true;
    }

    public static void MergeExitMusic(
        Dictionary<string, VmMapInfoExitMapTextEntry> table,
        string forMap,
        string music,
        int order)
    {
        if (!table.TryGetValue(forMap, out var entry))
        {
            entry = new VmMapInfoExitMapTextEntry();
            table[forMap] = entry;
        }

        entry.Music = music;
        entry.MusicOrder = order;
        entry.HasMusic = true;
    }

    public static void MergeExitBackdrop(
        Dictionary<string, VmMapInfoExitMapTextEntry> table,
        string forMap,
        string backdrop,
        bool isPic)
    {
        if (!table.TryGetValue(forMap, out var entry))
        {
            entry = new VmMapInfoExitMapTextEntry();
            table[forMap] = entry;
        }

        entry.Backdrop = backdrop;
        entry.BackdropIsPic = isPic;
        entry.HasBackdrop = true;
    }

    public static int ComposeEnvironmentId(int highByte, int? lowByte) =>
        (highByte << 8) | (lowByte ?? 0);

    public static int ResolveEnvironmentId(
        bool hasNumericHigh,
        int highByte,
        int? lowByte,
        string? namedEnvironment,
        Func<string, int?> findEnvironment)
    {
        if (hasNumericHigh)
            return ComposeEnvironmentId(highByte, lowByte);

        if (string.IsNullOrEmpty(namedEnvironment))
            return 0;

        return findEnvironment(namedEnvironment) ?? 0;
    }

    public static void ApplyDefaultEnvironment(VmMapInfoLevelInfo info, int id) => info.DefaultEnvironment = id;

    public static void ApplyHazardColor(VmMapInfoLevelInfo info, int color) => info.HazardColor = color;

    public static void ApplyHazardFlash(VmMapInfoLevelInfo info, int color) => info.HazardFlash = color;
}

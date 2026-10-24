namespace HCDE.Gamedata;

public sealed class VmMapInfoHeaderDraft
{
    public string MapName { get; set; } = "";
    public string LevelName { get; set; } = "";
    public bool LookupLevelName { get; set; }
    public int LevelNum { get; set; }
    public int Id24LevelNum { get; set; }
    public bool HexenHack { get; set; }
}

/// <summary><c>ParseMapHeader</c> helpers from <c>g_mapinfo.cpp</c> (2329–2443).</summary>
public static class VmMapInfoParseMapHeader
{
    public static string ResolveMapNameToken(bool isNumber, int number, string text, bool newFormat, ref bool hexenHack)
    {
        if (!isNumber)
            return text.ToUpperInvariant();

        if (newFormat)
            return text.ToUpperInvariant();

        hexenHack = true;
        return $"MAP{number:00}";
    }

    public static void ApplyHexenHackDefaults(VmMapInfoLevelFlags flags)
    {
        flags.Flags |= 0x0000020d;
        flags.Flags2 |= 0x00000035;
    }

    public static void ApplyLevelTitle(VmMapInfoHeaderDraft draft, string token, bool useLookupTable)
    {
        if (token.StartsWith('$'))
        {
            draft.LookupLevelName = true;
            draft.LevelName = token[1..];
            return;
        }

        if (token.Equals("lookup", StringComparison.OrdinalIgnoreCase))
        {
            draft.LookupLevelName = true;
            return;
        }

        draft.LevelName = token;
        if (useLookupTable)
            draft.LookupLevelName = true;
    }

    public static void AssignDefaultLevelNum(VmMapInfoHeaderDraft draft)
    {
        draft.LevelNum = VmEMapInfoParsePrimitives.GetDefaultLevelNum(draft.MapName, out var id24);
        draft.Id24LevelNum = id24;
    }
}

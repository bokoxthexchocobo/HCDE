namespace HCDE.Gamedata;

public sealed class VmMapInfoSpecialActionDraft
{
    public string Type { get; set; } = "";
    public int Action { get; set; }
    public int[] Args { get; } = new int[5];
    public int ArgCount { get; set; }
}

/// <summary>MAPINFO options <c>specialaction</c> through <c>f1</c> (<c>g_mapinfo.cpp</c> 1421–1560).</summary>
public static class VmMapInfoMapOptionsLate
{
    public static bool TryResolveSpecialAction(
        string lineSpecial,
        Func<string, (int action, int minArg, int maxArg)> findLineSpecial,
        out int action,
        out string? error)
    {
        action = 0;
        error = null;
        var (act, minArg, _) = findLineSpecial(lineSpecial);
        if (act == 0 || minArg < 0)
        {
            error = $"Unknown specialaction '{lineSpecial}'";
            return false;
        }

        action = act;
        return true;
    }

    public static void CollectSpecialActionArgs(IEnumerable<int> trailingNumbers, VmMapInfoSpecialActionDraft draft)
    {
        var i = 0;
        foreach (var n in trailingNumbers)
        {
            if (i >= draft.Args.Length) break;
            draft.Args[i++] = n;
        }

        draft.ArgCount = i;
    }

    public static List<int> ParsePrecacheSounds(
        IEnumerable<string> tokens,
        Func<string, int> resolveSound,
        int noSoundId)
    {
        var list = new List<int>();
        foreach (var token in tokens)
        {
            var id = resolveSound(token);
            if (id != noSoundId)
                list.Add(id);
        }

        return list;
    }

    public static List<string> ParseCommaSeparatedStrings(IEnumerable<string> tokens)
    {
        var list = new List<string>();
        foreach (var raw in tokens)
        {
            var token = raw.Trim();
            if (token == ",") continue;
            if (string.IsNullOrWhiteSpace(token)) break;
            list.Add(token);
        }

        return list;
    }

    public static void ApplyRedirect(VmMapInfoLevelOptionsDraft draft, string type, string map) =>
        (draft.AuthorName, draft.NextMap) = (type, map.Trim());

    public static void ApplyCvarRedirect(VmMapInfoLevelOptionsDraft draft, string cvar, string map) =>
        (draft.MapLabel, draft.LightningSound) = (cvar, map.Trim());

    public static void ApplyStringOption(VmMapInfoLevelOptionsDraft draft, string field, string value)
    {
        switch (field)
        {
            case "sndseq": draft.LightningSound = value; break;
            case "sndinfo":
            case "soundinfo": draft.AuthorName = value; break;
            case "translator": draft.MapLabel = value; break;
            case "deathsequence": draft.NextSecretMap = value; break;
            case "slideshow": draft.NextMap = value; break;
        }
    }

    public static void ApplyLumpOption(VmMapInfoLevelOptionsDraft draft, string field, string lump)
    {
        if (field == "bordertexture") draft.SkyPic1 = lump;
        if (field == "f1") draft.SkyPic2 = lump;
    }
}

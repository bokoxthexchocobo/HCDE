namespace HCDE.Gamedata;

public sealed class VmUMapInfoBossAction
{
    public string Actor { get; set; } = "";
    public int Special { get; set; }
    public int Tag { get; set; }
}

public sealed class VmUMapInfoMap
{
    public string MapName { get; set; } = "";
    public VmUMapEntry Entry { get; } = new();
    public List<VmUMapInfoBossAction> BossActions { get; } = [];
    public bool BossCleared { get; set; }
    public int Id24LevelNum { get; set; }
}

/// <summary>Remaining <c>umapinfo.cpp</c> map parse, lump ingest, and <c>CommitUMapinfo</c>.</summary>
public static class VmUMapInfoParser
{
    public static bool TryParseBossAction(IReadOnlyList<string> tokens, ref int i, VmUMapInfoMap map, List<string> errors)
    {
        if (i >= tokens.Count) return false;
        if (tokens[i].Equals("clear", StringComparison.OrdinalIgnoreCase))
        {
            map.BossActions.Clear();
            map.BossCleared = true;
            i++;
            return true;
        }

        var actor = tokens[i++];
        if (i + 4 >= tokens.Count || tokens[i] != ",") { errors.Add("Expected bossaction fields."); return false; }
        i++;
        var special = int.Parse(tokens[i++]);
        if (tokens[i] != ",") return false;
        i++;
        var tag = int.Parse(tokens[i++]);
        if (tag != 0 || special is 11 or 51 or 52 or 124)
            map.BossActions.Add(new VmUMapInfoBossAction { Actor = actor, Special = special | 0x40000000, Tag = tag });
        return true;
    }

    public static bool TryParseMapBlock(
        IReadOnlyList<string> tokens,
        ref int i,
        VmUMapInfoMap map,
        List<string> errors,
        ref int id24LevelNum,
        IList<VmUMapEpisode>? episodes = null)
    {
        if (i >= tokens.Count) return false;
        map.MapName = tokens[i++];
        map.Id24LevelNum = ++id24LevelNum;
        if (i >= tokens.Count || tokens[i] != "{") { errors.Add("Expected '{'."); return false; }
        i++;
        while (i < tokens.Count && tokens[i] != "}")
        {
            var prop = tokens[i++];
            if (prop.Equals("bossaction", StringComparison.OrdinalIgnoreCase))
            {
                if (!TryParseBossAction(tokens, ref i, map, errors)) return false;
                continue;
            }

            if (prop.Equals("episode", StringComparison.OrdinalIgnoreCase) && episodes is not null)
            {
                if (!VmUMapInfoStandardPropertyParser.TryApplyEpisodeProperty(tokens, ref i, map.Entry, map.MapName, episodes, ref id24LevelNum, errors))
                    return false;
                continue;
            }

            if (!VmUMapInfoStandardPropertyParser.TryApplyProperty(prop, tokens, ref i, map.Entry, errors))
                errors.Add($"Unknown UMapInfo property '{prop}'.");
        }

        if (i < tokens.Count && tokens[i] == "}") i++;
        return true;
    }

    public static void FinishParsedMap(VmUMapInfoMap map)
    {
        if (string.IsNullOrEmpty(map.Entry.EndPic)) return;
        map.Entry.NextMap = "";
        if (map.Entry.EndPic == "!") map.Entry.EndPic = "";
    }

    public static void ApplyEndPicOverrides(VmUMapInfoMap map)
    {
        if (map.Entry.EndPic.Length == 0) return;
        map.Entry.NextMap = "";
        if (map.Entry.EndPic == "!") map.Entry.EndPic = "";
    }

    public static void UpsertMap(List<VmUMapInfoMap> maps, VmUMapInfoMap parsed)
    {
        for (var i = 0; i < maps.Count; i++)
        {
            if (maps[i].MapName.Equals(parsed.MapName, StringComparison.OrdinalIgnoreCase))
            {
                maps[i] = parsed;
                return;
            }
        }

        maps.Add(parsed);
    }

    public static string? ResolveNextMapFromEndPic(string endPic) => endPic switch
    {
        "$CAST" => "enDSeQ:INTER_CAST",
        "$BUNNY" => "enDSeQ:INTER_BUNNY",
        "" or "-" => null,
        _ => $"enDSeQ:{endPic}"
    };

    public static void ApplyPlayerActionOption(VmUMapPlayerActionOption option, ref uint levelFlags, uint allowMask, uint disallowMask)
    {
        switch (option)
        {
            case VmUMapPlayerActionOption.Allow or VmUMapPlayerActionOption.Require:
                levelFlags &= ~disallowMask;
                break;
            case VmUMapPlayerActionOption.Disallow:
                levelFlags |= disallowMask;
                break;
        }
    }

    public static void CommitToLevelInfo(VmUMapInfoMap map, VmUMapLevelCommit target, string defaultInterMusic = "")
    {
        if (!string.IsNullOrEmpty(map.MapName)) target.MapName = map.MapName;
        if (!string.IsNullOrEmpty(map.Entry.LevelName))
        {
            target.LevelName = map.Entry.LevelName;
            target.PName = "";
        }

        if (!string.IsNullOrEmpty(map.Entry.Author)) target.AuthorName = map.Entry.Author;
        if (!string.IsNullOrEmpty(map.Entry.Label)) target.MapLabel = map.Entry.Label;
        if (!string.IsNullOrEmpty(map.Entry.NextMap)) target.NextMap = map.Entry.NextMap;
        else if (ResolveNextMapFromEndPic(map.Entry.EndPic) is { } next) target.NextMap = next;
        if (map.Entry.Partime > 0) target.Partime = map.Entry.Partime;
        if (map.Entry.NoIntermission != 0) target.Flags |= 1u << 0;
        target.Id24LevelNum = map.Id24LevelNum;

        if (map.BossActions.Count > 0 || map.BossCleared)
        {
            target.BossActionCount = map.BossActions.Count;
            target.BossCleared = map.BossCleared;
        }

        if (!string.IsNullOrEmpty(map.Entry.InterText))
        {
            target.InterText = map.Entry.InterText == "-" ? "" : map.Entry.InterText;
            target.InterBackdrop = map.Entry.InterBackdrop;
            target.InterMusic = string.IsNullOrEmpty(map.Entry.InterMusic) ? defaultInterMusic : map.Entry.InterMusic;
        }

        if (!string.IsNullOrEmpty(map.Entry.InterTextSecret))
        {
            target.InterTextSecret = map.Entry.InterTextSecret == "-" ? "" : map.Entry.InterTextSecret;
        }

        var flags = target.Flags;
        ApplyPlayerActionOption(map.Entry.Jumping, ref flags, 1u << 1, 1u << 1);
        ApplyPlayerActionOption(map.Entry.Crouching, ref flags, 1u << 2, 1u << 2);
        ApplyFreeAimOption(map.Entry.FreeAim, ref flags);
        target.Flags = flags;
        target.Flags2 |= 1u << 0;
    }

    private static void ApplyFreeAimOption(VmUMapPlayerActionOption option, ref uint levelFlags)
    {
        const uint freelookNo = 1u << 3;
        const uint freelookYes = 1u << 4;
        switch (option)
        {
            case VmUMapPlayerActionOption.Allow:
                levelFlags &= ~(freelookNo | freelookYes);
                break;
            case VmUMapPlayerActionOption.Require:
                levelFlags |= freelookYes;
                levelFlags &= ~freelookNo;
                break;
            case VmUMapPlayerActionOption.Disallow:
                levelFlags |= freelookNo;
                levelFlags &= ~freelookYes;
                break;
        }
    }
}

public sealed class VmUMapLevelCommit
{
    public string MapName { get; set; } = "";
    public string LevelName { get; set; } = "";
    public string PName { get; set; } = "";
    public string AuthorName { get; set; } = "";
    public string MapLabel { get; set; } = "";
    public string NextMap { get; set; } = "";
    public int Partime { get; set; }
    public uint Flags { get; set; }
    public uint Flags2 { get; set; }
    public int Id24LevelNum { get; set; }
    public int BossActionCount { get; set; }
    public bool BossCleared { get; set; }
    public string InterText { get; set; } = "";
    public string InterTextSecret { get; set; } = "";
    public string InterBackdrop { get; set; } = "";
    public string InterMusic { get; set; } = "";
}

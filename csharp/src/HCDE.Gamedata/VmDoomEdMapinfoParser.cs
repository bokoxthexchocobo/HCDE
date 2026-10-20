namespace HCDE.Gamedata;

public sealed class VmMapinfoEdMapItem
{
    public string? ClassName { get; set; }
    public int Special { get; set; } = -1;
    public sbyte ArgsDefined { get; set; }
    public int[] Args { get; } = new int[5];
    public bool NoSkillFlags { get; set; }
    public int LineNum { get; set; }
}

/// <summary><c>SpecialMapthingNames</c> and <c>ParseDoomEdNums</c> from <c>g_doomedmap.cpp</c>.</summary>
public static class VmDoomEdMapinfoParser
{
    public static readonly string[] SpecialMapthingNames =
    [
        "$Player1Start", "$Player2Start", "$Player3Start", "$Player4Start",
        "$Player5Start", "$Player6Start", "$Player7Start", "$Player8Start",
        "$DeathmatchStart", "$SSeqOverride", "$PolyAnchor", "$PolySpawn",
        "$PolySpawnCrush", "$PolySpawnHurt", "$SlopeFloorPointLine", "$SlopeCeilingPointLine",
        "$SetFloorSlope", "$SetCeilingSlope", "$VavoomFloor", "$VavoomCeiling",
        "$CopyFloorPlane", "$CopyCeilingPlane", "$VertexFloorZ", "$VertexCeilingZ", "$EDThing"
    ];

    public static int MatchSpecialMapthing(string token)
    {
        if (token.Length == 0 || token[0] != '$') return -1;
        for (var i = 0; i < SpecialMapthingNames.Length; i++)
        {
            if (token.Equals(SpecialMapthingNames[i], StringComparison.OrdinalIgnoreCase))
                return i + 1;
        }

        return -1;
    }

    public static bool TryParseDoomEdNums(
        IReadOnlyList<string> tokens,
        ref int i,
        Dictionary<int, VmMapinfoEdMapItem> dest,
        Func<string, (int special, int minArgs, int maxArgs)>? resolveLineSpecial,
        List<string> errors)
    {
        if (i >= tokens.Count || tokens[i] != "{") { errors.Add("Expected '{'."); return false; }
        i++;
        var errorCount = 0;
        while (i < tokens.Count && tokens[i] != "}")
        {
            if (!int.TryParse(tokens[i++], out var edNum))
            {
                errors.Add("Number expected.");
                return false;
            }

            if (i >= tokens.Count || tokens[i] != "=") { errors.Add("Expected '='."); return false; }
            i++;
            if (i >= tokens.Count) { errors.Add("Expected class or special."); return false; }
            var name = tokens[i++];
            var item = new VmMapinfoEdMapItem { LineNum = i };
            var special = MatchSpecialMapthing(name);
            if (special > 0)
            {
                item.Special = special;
            }
            else
            {
                item.ClassName = name;
                item.Special = -1;
            }

            if (dest.ContainsKey(edNum))
            {
                errors.Add($"Editor Number {edNum} defined more than once");
                errorCount++;
            }

            item.ArgsDefined = 0;
            item.NoSkillFlags = false;
            var minArgs = 0;
            var maxArgs = 5;
            string? specialName = null;
            if (i < tokens.Count && tokens[i] == ",")
            {
                i++;
                if (i < tokens.Count && tokens[i].Equals("noskillflags", StringComparison.OrdinalIgnoreCase))
                {
                    item.NoSkillFlags = true;
                    i++;
                    if (i >= tokens.Count || tokens[i] != ",") goto finishEntry;
                    i++;
                }

                item.ArgsDefined = 5;
                if (item.Special < 0) item.Special = 0;
                if (i < tokens.Count && int.TryParse(tokens[i], out var firstArg))
                {
                    i++;
                    item.Args[0] = firstArg;
                }
                else if (i < tokens.Count && resolveLineSpecial is not null)
                {
                    specialName = tokens[i++];
                    var resolved = resolveLineSpecial(specialName);
                    item.Special = resolved.special;
                    minArgs = resolved.minArgs;
                    maxArgs = resolved.maxArgs;
                    if (item.Special == 0 || minArgs == -1)
                    {
                        errors.Add($"Invalid special {specialName} for Editor Number {edNum}");
                        errorCount++;
                        minArgs = 0;
                        maxArgs = 5;
                    }

                    if (i >= tokens.Count || tokens[i] != ",")
                    {
                        if (minArgs != 0)
                        {
                            errors.Add($"Incorrect number of args for special {specialName}");
                            errorCount++;
                        }

                        dest[edNum] = item;
                        continue;
                    }

                    i++;
                }
                else
                {
                    errors.Add("Expected argument list.");
                    return false;
                }

                var argIndex = 1;
                while (argIndex < 5)
                {
                    if (i >= tokens.Count || !int.TryParse(tokens[i], out var argVal)) break;
                    item.Args[argIndex++] = argVal;
                    i++;
                    if (i >= tokens.Count || tokens[i] != ",") break;
                    i++;
                    if (i < tokens.Count && tokens[i] == "+")
                    {
                        item.ArgsDefined = (sbyte)argIndex;
                        i++;
                        break;
                    }
                }

                if (specialName is not null && (argIndex < minArgs || argIndex > maxArgs))
                {
                    errors.Add($"Incorrect number of args for special {specialName}");
                    errorCount++;
                }
            }

        finishEntry:
            dest[edNum] = item;
        }

        if (i < tokens.Count && tokens[i] == "}") i++;
        if (errorCount > 0) errors.Add($"{errorCount} errors encountered in DoomEdNum definition");
        return errorCount == 0;
    }

    public static IEnumerable<(int EdNum, string Label)> EnumerateSortedForDump(IReadOnlyDictionary<int, VmMapinfoEdMapItem> map)
    {
        foreach (var key in map.Keys.OrderBy(k => k))
        {
            var item = map[key];
            string label;
            if (item.ClassName is not null) label = item.ClassName;
            else if (item.Special > 0 && item.Special <= SpecialMapthingNames.Length)
                label = SpecialMapthingNames[item.Special - 1];
            else label = "none";
            yield return (key, label);
        }
    }
}

namespace HCDE.Gamedata;

public sealed class VmSwitchDef
{
    public int PreTextureIndex { get; set; }
    public string Sound { get; set; } = "";
    public List<VmSwitchFrame> Frames { get; } = [];
    public VmSwitchDef? Pair { get; set; }
}

public sealed class VmSwitchFrame
{
    public int TextureIndex { get; set; }
    public uint TimeMin { get; set; }
    public uint TimeRnd { get; set; }
}

public static class VmAnimSwitchRuntime
{
    public static IReadOnlyList<(string Off, string On)> ParseSwitchLumpEntries(string lumpText)
    {
        var list = new List<(string, string)>();
        for (var i = 0; i + 19 < lumpText.Length; i += 20)
        {
            var off = lumpText.Substring(i, 9).TrimEnd('\0', ' ');
            var on = lumpText.Substring(i + 9, 9).TrimEnd('\0', ' ');
            if (off.Length == 0 && on.Length == 0) break;
            if (off.Equals(on, StringComparison.OrdinalIgnoreCase)) continue;
            list.Add((off, on));
        }

        return list;
    }

    public static void AddSwitchPair(List<VmSwitchDef> defs, VmSwitchDef def1, VmSwitchDef def2)
    {
        def1.Pair = def2;
        def2.Pair = def1;
        var index1 = defs.FindIndex(d => d.PreTextureIndex == def1.PreTextureIndex);
        var index2 = defs.FindIndex(d => d.PreTextureIndex == def2.PreTextureIndex);
        if (index1 >= 0 && index2 >= 0 && defs[index1].Pair == defs[index2] && defs[index2].Pair == defs[index1])
        {
            defs[index1] = def1;
            defs[index2] = def2;
            return;
        }

        if (index1 >= 0) defs[index1].PreTextureIndex = -1;
        if (index2 >= 0) defs[index2].PreTextureIndex = -1;
        defs.Add(def1);
        defs.Add(def2);
        defs.Sort((a, b) => a.PreTextureIndex.CompareTo(b.PreTextureIndex));
    }

    public static VmSwitchDef? FindSwitch(IReadOnlyList<VmSwitchDef> defs, int textureIndex)
    {
        var low = 0;
        var high = defs.Count - 1;
        while (low <= high)
        {
            var mid = (low + high) / 2;
            var pre = defs[mid].PreTextureIndex;
            if (pre == textureIndex) return defs[mid];
            if (textureIndex < pre) high = mid - 1;
            else low = mid + 1;
        }

        return null;
    }

    public static bool TryParseSwitchStateFrames(
        IReadOnlyList<string> tokens,
        ref int i,
        IAnimDefTextureCatalog textures,
        VmSwitchDef def,
        List<string> errors)
    {
        while (i < tokens.Count)
        {
            if (tokens[i].Equals("sound", StringComparison.OrdinalIgnoreCase))
            {
                i++;
                if (i < tokens.Count) def.Sound = tokens[i++];
                continue;
            }

            if (tokens[i].Equals("pic", StringComparison.OrdinalIgnoreCase))
            {
                i++;
                if (i >= tokens.Count) { errors.Add("Expected switch pic texture."); return false; }
                var tex = tokens[i++];
                var index = textures.GetTextureIndex(tex, isFlat: false);
                if (!VmAnimDefDocumentParser.TryParseTime(tokens, ref i, out var min, out var max, errors)) return false;
                def.Frames.Add(new VmSwitchFrame { TextureIndex = index, TimeMin = min, TimeRnd = max > min ? max - min : 0 });
                continue;
            }

            break;
        }

        if (def.Frames.Count == 0)
            errors.Add("Switch state needs at least one frame");
        return errors.Count == 0;
    }
}

namespace HCDE.Gamedata;

public sealed record VmSndInfoPlayerClass(string Name, ushort[] GenderListIndex);

public static class VmSndInfoPlayerClassRegistry
{
    public static bool TryParsePlayerSoundCommon(
        IReadOnlyList<string> tokens,
        ref int index,
        out string playerClass,
        out int gender,
        out string logicalSlot,
        List<string> errors)
    {
        playerClass = "";
        gender = 0;
        logicalSlot = "";
        if (index + 2 >= tokens.Count) { errors.Add("Player sound requires class, gender, and slot."); return false; }
        playerClass = Unquote(tokens[index++]);
        gender = VmSndInfoGender.Parse(tokens[index++], errors);
        if (errors.Count > 0) return false;
        logicalSlot = tokens[index++];
        return true;
    }

    public static int AddPlayerClass(List<VmSndInfoPlayerClass> classes, string name)
    {
        var existing = FindPlayerClass(classes, name);
        if (existing >= 0) return existing;
        classes.Add(new VmSndInfoPlayerClass(name, Enumerable.Repeat<ushort>(0xffff, 4).ToArray()));
        return classes.Count - 1;
    }

    public static int FindPlayerClass(IReadOnlyList<VmSndInfoPlayerClass> classes, string name)
    {
        for (var i = 0; i < classes.Count; i++)
        {
            if (classes[i].Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                return i;
        }

        return -1;
    }

    public static int FindPlayerClassSorted(IReadOnlyList<VmSndInfoPlayerClass> sorted, string name)
    {
        var low = 0;
        var high = sorted.Count - 1;
        while (low <= high)
        {
            var mid = (low + high) / 2;
            var cmp = string.Compare(name, sorted[mid].Name, StringComparison.OrdinalIgnoreCase);
            if (cmp == 0) return mid;
            if (cmp < 0) high = mid - 1;
            else low = mid + 1;
        }

        return -1;
    }

    private static string Unquote(string token) => token.StartsWith('"') ? token[1..^1] : token;
}

public static class VmSndInfoGender
{
    public static int Parse(string token, List<string> errors)
    {
        if (token.Equals("male", StringComparison.OrdinalIgnoreCase)) return 0;
        if (token.Equals("female", StringComparison.OrdinalIgnoreCase)) return 1;
        if (token.Equals("other", StringComparison.OrdinalIgnoreCase)) return 2;
        errors.Add($"Unknown player gender '{token}'.");
        return 0;
    }
}

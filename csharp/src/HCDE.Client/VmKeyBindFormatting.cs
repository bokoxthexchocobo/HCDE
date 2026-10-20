namespace HCDE.Client;

/// <summary><c>C_NameKeys</c> from <c>c_bind.cpp</c>.</summary>
public static class VmKeyBindFormatting
{
    public static string NameKeys(int first, int second, bool colors = false)
    {
        if (second == first) second = 0;
        if (first == 0 && second == 0) return "";
        var sep = colors ? "\u001c,\u001c " : ", ";
        if (first == 0) return VmKeyNames.KeyName(second);
        if (second == 0) return VmKeyNames.KeyName(first);
        return VmKeyNames.KeyName(first) + sep + VmKeyNames.KeyName(second);
    }

    public static string NameKeys(IReadOnlyList<int> keys, bool colors = false)
    {
        var parts = new List<string>();
        for (var i = 0; i < keys.Count; i++)
        {
            var key = keys[i];
            if (key == 0) continue;
            if (keys.Take(i).Contains(key)) continue;
            parts.Add(VmKeyNames.KeyName(key));
        }

        var sep = colors ? "\u001c,\u001c " : ", ";
        return string.Join(sep, parts);
    }
}

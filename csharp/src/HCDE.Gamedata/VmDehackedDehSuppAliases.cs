namespace HCDE.Gamedata;

public readonly record struct VmDehackedCodePointerAlias(string Alias, string Name);

/// <summary><c>LoadDehSupp</c> <c>Aliases</c> section from <c>d_dehacked.cpp</c>.</summary>
public static class VmDehackedDehSuppAliases
{
    public static bool TryParseAliasPair(string alias, string name, out VmDehackedCodePointerAlias pair)
    {
        pair = default;
        if (string.IsNullOrWhiteSpace(alias) || string.IsNullOrWhiteSpace(name))
            return false;
        pair = new VmDehackedCodePointerAlias(alias.Trim(), name.Trim());
        return true;
    }

    public static List<VmDehackedCodePointerAlias> ParseAliasEntries(IEnumerable<(string Alias, string Name)> rows)
    {
        var list = new List<VmDehackedCodePointerAlias>();
        foreach (var (alias, name) in rows)
        {
            if (!TryParseAliasPair(alias, name, out var pair))
                break;
            list.Add(pair);
        }

        return list;
    }
}

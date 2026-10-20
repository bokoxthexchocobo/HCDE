namespace HCDE.Gamedata;

/// <summary><c>LoadDehSupp</c> <c>OrgSprNames</c> from <c>d_dehacked.cpp</c>.</summary>
public static class VmDehackedDehSuppOrgSprNames
{
    public static bool TryParseSpriteName(string token, out string? name)
    {
        name = null;
        if (token.Length != 4)
            return false;
        name = token;
        return true;
    }

    public static List<string> ParseSpriteNames(IEnumerable<string> tokens, bool appendOnlyWhenEmpty, IList<string>? existing)
    {
        if (!appendOnlyWhenEmpty || existing is { Count: > 0 })
            return existing is null ? [] : [.. existing];
        var list = new List<string>();
        foreach (var raw in tokens)
        {
            var token = raw.Trim();
            if (token == ",") continue;
            if (!TryParseSpriteName(token, out var name) || name is null)
                break;
            list.Add(name);
        }

        return list;
    }
}

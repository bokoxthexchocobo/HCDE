namespace HCDE.Gamedata;

/// <summary><c>LoadDehSupp</c> <c>AmmoNames</c> section from <c>d_dehacked.cpp</c>.</summary>
public static class VmDehackedDehSuppAmmoNames
{
    public static bool TryParseAmmoEntry(string token, out string? ammoType)
    {
        ammoType = null;
        if (token.Equals("NULL", StringComparison.OrdinalIgnoreCase))
            return true;
        if (token.Length == 0)
            return false;
        ammoType = token;
        return true;
    }

    public static bool IsCommaSeparator(string token) => token == ",";

    public static List<string?> ParseAmmoNameList(IEnumerable<string> tokens)
    {
        var result = new List<string?>();
        foreach (var raw in tokens)
        {
            var token = raw.Trim();
            if (IsCommaSeparator(token))
                continue;
            if (!TryParseAmmoEntry(token, out var ammo))
                break;
            result.Add(ammo);
        }

        return result;
    }
}

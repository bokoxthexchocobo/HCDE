namespace HCDE.Gamedata;

/// <summary><c>LoadDehSupp</c> <c>WeaponNames</c> section from <c>d_dehacked.cpp</c>.</summary>
public static class VmDehackedDehSuppWeaponNames
{
    public static bool TryParseWeaponEntry(string token, out string? weaponType)
    {
        weaponType = null;
        if (token.Length == 0)
            return false;
        weaponType = token;
        return true;
    }

    public static bool IsCommaSeparator(string token) => token == ",";

    public static List<string> ParseWeaponNameList(IEnumerable<string> tokens)
    {
        var result = new List<string>();
        foreach (var raw in tokens)
        {
            var token = raw.Trim();
            if (IsCommaSeparator(token))
                continue;
            if (!TryParseWeaponEntry(token, out var weapon) || weapon is null)
                break;
            result.Add(weapon);
        }

        return result;
    }
}

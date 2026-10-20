namespace HCDE.Gamedata;

/// <summary><c>LoadDehSupp</c> <c>OrgSprNames</c> from <c>d_dehacked.cpp</c> (3570–3597).</summary>
public static class VmDehackedDehSuppOrgSprNames
{
    public const string InvalidSpriteNameMessage = "Invalid sprite name (must be 4 characters)";

    public static bool TryParseSpriteName(string token, out string? name)
    {
        name = null;
        if (token.Length != 4)
            return false;
        name = token;
        return true;
    }

    public static bool ShouldAppendToOrgList(int existingCount) => existingCount == 0;

    public static bool TryParseOrgSprNamesSection(
        IReadOnlyList<string> tokens,
        IReadOnlyList<string> existingOrgSprNames,
        out List<string> orgSprNames,
        out int orgSprOrgSize,
        out string? error)
    {
        orgSprNames = existingOrgSprNames.Count == 0 ? [] : [.. existingOrgSprNames];
        orgSprOrgSize = orgSprNames.Count;
        error = null;
        if (!ShouldAppendToOrgList(existingOrgSprNames.Count))
            return true;

        var parsed = new List<string>();
        foreach (var raw in tokens)
        {
            var token = raw.Trim();
            if (token == "," || token == "{" || token == "}")
                continue;
            if (!TryParseSpriteName(token, out var name) || name is null)
            {
                error = InvalidSpriteNameMessage;
                return false;
            }

            parsed.Add(name);
        }

        orgSprNames = parsed;
        orgSprOrgSize = orgSprNames.Count;
        return true;
    }
}

namespace HCDE.Gamedata;

/// <summary><c>DIntermissionScreen::Init</c> background resolution from <c>intermission.cpp</c>.</summary>
public static class VmIntermissionScreenInit
{
    public static string ResolveBackground(string texname, IReadOnlyList<string> finalePages, string titlePage,
        Func<string, string?> localizeString)
    {
        if (texname.Length == 0) return "";
        if (texname[0] == '@')
        {
            if (uint.TryParse(texname.AsSpan(1), out var page) && page > 0 && page - 1 < (uint)finalePages.Count)
                return finalePages[(int)page - 1];
            if (finalePages.Count > 0) return finalePages[0];
            return titlePage;
        }

        if (texname[0] == '$')
            return localizeString(texname[1..]) ?? texname;

        return texname;
    }
}

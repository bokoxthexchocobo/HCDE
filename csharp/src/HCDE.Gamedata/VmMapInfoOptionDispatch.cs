namespace HCDE.Gamedata;

public sealed class VmMapInfoOptionRegistration
{
    public string Name { get; init; } = "";
    public bool RequiresNewFormat { get; init; }
}

/// <summary>MAPINFO unknown option dispatch from <c>g_mapinfo.cpp</c>.</summary>
public static class VmMapInfoOptionDispatch
{
    public static bool TryDispatchOption(
        string token,
        bool newFormat,
        IReadOnlyList<VmMapInfoOptionRegistration> options,
        out VmMapInfoOptionRegistration? matched)
    {
        foreach (var option in options)
        {
            if (!option.Name.Equals(token, StringComparison.OrdinalIgnoreCase)) continue;
            if (option.RequiresNewFormat && !newFormat)
                throw new InvalidOperationException($"MAPINFO option '{token}' requires the new MAPINFO format");
            matched = option;
            return true;
        }

        matched = null;
        return false;
    }
}

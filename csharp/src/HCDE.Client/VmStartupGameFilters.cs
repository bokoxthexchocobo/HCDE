namespace HCDE.Client;

public enum VmStartupGameType { Doom = 1, Heretic = 2, Hexen = 4, Strife = 8, Chex = 16 }

/// <summary>Builds the ordered archive filters used by native startup.</summary>
public static class VmStartupGameFilters
{
    public static IReadOnlyList<string> Build(VmStartupGameType game, string iwadFilter = "")
    {
        if (!Enum.IsDefined(game)) throw new ArgumentOutOfRangeException(nameof(game));
        ArgumentNullException.ThrowIfNull(iwadFilter);
        var filters = new List<string>();
        if (game is VmStartupGameType.Heretic or VmStartupGameType.Hexen) filters.Add("game-Raven");
        if (game is VmStartupGameType.Doom or VmStartupGameType.Strife or VmStartupGameType.Chex)
            filters.Add("game-DoomStrifeChex");
        if (game is VmStartupGameType.Doom or VmStartupGameType.Chex) filters.Add("game-DoomChex");
        filters.Add("game-" + game);
        filters.Add(iwadFilter);
        // Native code checks for the substring, but slices at the fixed offset 12.
        if (iwadFilter.Contains("doom.id.doom", StringComparison.Ordinal))
            filters.Add("doom.doom" + iwadFilter[12..]);
        return filters.AsReadOnly();
    }
}

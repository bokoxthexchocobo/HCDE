namespace HCDE.Client;

/// <summary><c>ParseGameInfo</c>, <c>GetReserved</c>, <c>CheckGameInfo</c>, <c>SetMapxxFlag</c> from <c>d_main.cpp</c> (2311–2480).</summary>
public static class VmDMainGameInfoScan
{
    public enum StartupType
    {
        Default,
        Doom,
        Heretic,
        Hexen,
        Strife
    }

    public readonly record struct GameInfoParseState(
        string Iwad,
        string StartupName,
        StartupType StartupType,
        int FgColor,
        int BgColor,
        string StartupSong,
        int LoadLights,
        int LoadBrightmaps,
        int LoadWidescreen,
        string SteamAppId,
        bool NoSpriteRename);

    public static string ResolveLoadPath(string gameInfoPath, string loadName, Func<string, bool> pathExists)
    {
        var lastSlash = Math.Max(gameInfoPath.LastIndexOf('/'), gameInfoPath.LastIndexOf(':'));
        var checkPath = lastSlash >= 0
            ? gameInfoPath[..(lastSlash + 1)] + loadName
            : loadName;
        return pathExists(checkPath) ? checkPath : loadName;
    }

    public static StartupType ParseStartupType(string token) =>
        token.ToUpperInvariant() switch
        {
            "DOOM" => StartupType.Doom,
            "HERETIC" => StartupType.Heretic,
            "HEXEN" => StartupType.Hexen,
            "STRIFE" => StartupType.Strife,
            _ => StartupType.Default
        };

    public static bool TryApplyGameInfoKey(
        string key,
        IReadOnlyList<string> values,
        ref GameInfoParseState state)
    {
        if (key.Equals("IWAD", StringComparison.OrdinalIgnoreCase) && values.Count > 0)
        {
            state = state with { Iwad = values[0] };
            return true;
        }

        if (key.Equals("NOSPRITERENAME", StringComparison.OrdinalIgnoreCase) && values.Count > 0)
        {
            state = state with { NoSpriteRename = values[0].Equals("true", StringComparison.OrdinalIgnoreCase) };
            return true;
        }

        if (key.Equals("STARTUPTITLE", StringComparison.OrdinalIgnoreCase) && values.Count > 0)
        {
            state = state with { StartupName = values[0] };
            return true;
        }

        if (key.Equals("STARTUPTYPE", StringComparison.OrdinalIgnoreCase) && values.Count > 0)
        {
            state = state with { StartupType = ParseStartupType(values[0]) };
            return true;
        }

        if (key.Equals("STARTUPSONG", StringComparison.OrdinalIgnoreCase) && values.Count > 0)
        {
            state = state with { StartupSong = values[0] };
            return true;
        }

        if (key.Equals("STEAMAPPID", StringComparison.OrdinalIgnoreCase) && values.Count > 0)
        {
            state = state with { SteamAppId = values[0] };
            return true;
        }

        if (key.Equals("LOADLIGHTS", StringComparison.OrdinalIgnoreCase) && values.Count > 0
            && int.TryParse(values[0], out var lights))
        {
            state = state with { LoadLights = lights != 0 ? 1 : 0 };
            return true;
        }

        if (key.Equals("LOADBRIGHTMAPS", StringComparison.OrdinalIgnoreCase) && values.Count > 0
            && int.TryParse(values[0], out var bright))
        {
            state = state with { LoadBrightmaps = bright != 0 ? 1 : 0 };
            return true;
        }

        if (key.Equals("LOADWIDESCREEN", StringComparison.OrdinalIgnoreCase) && values.Count > 0
            && int.TryParse(values[0], out var wide))
        {
            state = state with { LoadWidescreen = wide != 0 ? 1 : 0 };
            return true;
        }

        return false;
    }

    public static IReadOnlyList<string> ReservedFolders => new[]
    {
        "flats/", "textures/", "hires/", "sprites/", "voxels/", "colormaps/", "acs/", "maps/",
        "voices/", "patches/", "graphics/", "sounds/", "music/", "materials/", "models/", "fonts/",
        "brightmaps/"
    };

    public static IReadOnlyList<string> RequiredPrefixes => new[]
    {
        "mapinfo", "zmapinfo", "umapinfo", "emapinfo", "gameinfo", "sndinfo", "sndseq", "sbarinfo",
        "menudef", "gldefs", "animdefs", "decorate", "zscript", "iwadinfo", "complvl", "terrain", "maps/"
    };

    public static IReadOnlyList<string> BlockedNames => new[]
    {
        "*.bat", "*.exe", "__macosx/*", "*/__macosx/*", "*~"
    };

    public static bool ShouldSetMapxxFlag(bool hasMap01Lump, bool hasMap01Wad, bool hasMap01Map) =>
        hasMap01Lump || hasMap01Wad || hasMap01Map;
}

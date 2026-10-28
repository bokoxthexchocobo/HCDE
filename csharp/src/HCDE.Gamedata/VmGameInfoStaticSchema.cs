namespace HCDE.Gamedata;

/// <summary>Static GAMEINFO tables and macro key catalog from <c>gi.cpp</c>.</summary>
public static class VmGameInfoStaticSchema
{
    public static IReadOnlyDictionary<int, string> GameNames => new Dictionary<int, string>
    {
        [1] = "Doom",
        [2] = "Heretic",
        [4] = "Hexen",
        [8] = "Strife",
        [16] = "Chex"
    };

    public static IReadOnlyList<string> GameInfoBorderPresets => ["DoomBorder", "HereticBorder", "StrifeBorder"];

    public static VmGameBorder ResolveBorderPreset(string preset)
    {
        if (preset.Equals("HereticBorder", StringComparison.OrdinalIgnoreCase))
            return new VmGameBorder { Size = 4, Offset = 16 };
        return new VmGameBorder { Size = 8, Offset = 8 };
    }

    public static IReadOnlyList<string> GameInfoMacroKeys =>
    [
        "titlePage", "titleMusic", "finalePage", "defaultrespawntime", "menufontcolor_title",
        "statscreen_mapnamefont", "bloodsplatdecaldistance", "normforwardmove"
    ];
}

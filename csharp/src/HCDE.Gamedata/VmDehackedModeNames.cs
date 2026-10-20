namespace HCDE.Gamedata;

/// <summary>DEH/BEX chunk mode names from <c>Modes[]</c> in <c>d_dehacked.cpp</c>.</summary>
public static class VmDehackedModeNames
{
    public static IReadOnlyList<string> PatchModes =>
    [
        "Thing", "Sound", "Frame", "Sprite", "Ammo", "Weapon", "Pointer", "Cheat", "Misc", "Text",
        "include", "[STRINGS]", "[PARS]", "[CODEPTR]", "[MUSIC]", "[SPRITES]", "[SOUNDS]"
    ];

    public static bool IsKnownMode(string mode) =>
        PatchModes.Contains(mode, StringComparer.OrdinalIgnoreCase);
}

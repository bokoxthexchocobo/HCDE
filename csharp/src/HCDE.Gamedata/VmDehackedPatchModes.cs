namespace HCDE.Gamedata;

/// <summary>DeHackEd section dispatch table from <c>d_dehacked.cpp</c> (463–478).</summary>
public static class VmDehackedPatchModes
{
    public static IReadOnlyList<string> CoreSectionNames =>
    [
        "Thing", "Sound", "Frame", "Sprite", "Ammo", "Weapon", "Pointer",
        "Cheat", "Misc", "Text", "include", "Pars", "CodePtr", "Music",
        "SpriteNames", "SoundNames"
    ];

    public static bool IsKnownSection(string name) =>
        CoreSectionNames.Any(s => s.Equals(name, StringComparison.OrdinalIgnoreCase));
}

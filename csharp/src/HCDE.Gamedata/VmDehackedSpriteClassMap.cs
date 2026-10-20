namespace HCDE.Gamedata;

/// <summary><c>DehSpriteMappings</c> from <c>d_dehacked.cpp</c> (372–415).</summary>
public static class VmDehackedSpriteClassMap
{
    private static readonly Dictionary<string, string> Map = new(StringComparer.Ordinal)
    {
        ["AMMO"] = "ClipBox",
        ["SOUL"] = "Soulsphere",
        ["BON1"] = "HealthBonus",
        ["SHOT"] = "Shotgun",
    };

    public static bool TryResolveClass(string sprite, out string className) =>
        Map.TryGetValue(sprite.ToUpperInvariant(), out className!);
}

namespace HCDE.Gamedata;

/// <summary><c>PatchMisc</c> powerup color lines from <c>d_dehacked.cpp</c>.</summary>
public static class VmDehackedPatchMiscPowerups
{
    public static readonly string[] PowerupNames =
    [
        "Invulnerability",
        "Berserk",
        "Invisibility",
        "Radiation Suit",
        "Infrared",
        "Tome of Power",
        "Wings of Wrath",
        "Speed",
        "Minotaur"
    ];

    public static bool TryMapPowerupIndex(string key, out int index)
    {
        const string prefix = "Powerup Color ";
        if (!key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) { index = -1; return false; }
        var name = key[prefix.Length..];
        for (var i = 0; i < PowerupNames.Length; i++)
            if (PowerupNames[i].Equals(name, StringComparison.OrdinalIgnoreCase)) { index = i; return true; }
        index = -1;
        return false;
    }

    public static bool TryParseRgba(string text, out int r, out int g, out int b, out float a)
    {
        r = g = b = 0;
        a = 0;
        var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 4) return false;
        if (!int.TryParse(parts[0], out r) || !int.TryParse(parts[1], out g) || !int.TryParse(parts[2], out b)) return false;
        return float.TryParse(parts[3], out a);
    }
}

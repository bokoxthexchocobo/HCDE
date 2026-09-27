namespace HCDE.Playsim;

/// <summary>
/// Server-side compatibility switches. These gate a few specials. They are not an MBF21, ID24, or Eternity port.
/// </summary>
[Flags]
public enum CompatSurface
{
    None = 0,
    Mbf21 = 1,
    Id24 = 2,
    Eternity = 4,
    SharedLightMaximum = 8,
    LegacyScriptWaitDirect = 16,
    // Selects native Hexen defaults for converted crush/stop modes, independent of map format.
    HexenCrushDefaults = 32,
}

public static class CompatSurfaceRules
{
    public const int EternityReverbSpecial = 200;
    public const int Id24SecretExit = 243;
    public const int Mbf21FloorNudge = 270;
}

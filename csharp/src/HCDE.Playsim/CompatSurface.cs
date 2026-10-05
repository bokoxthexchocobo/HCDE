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
    /// <summary>Native <c>COMPATF_BOOMSCROLL</c>. Non-players sum multi-sector carry; players still average.</summary>
    BoomScroll = 64,
    /// <summary>Native <c>COMPATF_NOTOSSDROPS</c>. Drops use source Z without toss velocity.</summary>
    NoTossDrops = 128,
    /// <summary>Native <c>COMPATF_LIMITPAIN</c>. Pain Elementals stop spawning when 21 Lost Souls exist.</summary>
    LimitPain = 256,
    /// <summary>Native COMPATF_VILEGHOSTS: archvile resurrection retains corpse radius and quadruples corpse height.</summary>
    VileGhosts = 512,
    DehHealth = 1024,
    /// <summary>Native COMPATF_MISSILECLIP: negative projectile pass heights use their absolute height.</summary>
    MissileClip = 2048,
}

public static class CompatSurfaceRules
{
    public const int EternityReverbSpecial = 200;
    public const int Id24SecretExit = 243;
    public const int Mbf21FloorNudge = 270;
}

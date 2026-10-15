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
    /// <summary>Native COMPATF_USEBLOCKING: use-through takes precedence and other special lines block use.</summary>
    UseBlocking = 4096,
    /// <summary>Native COMPATF2_POINTONLINE: activation sides use the vanilla classifier.</summary>
    PointOnLine = 8192,
    /// <summary>Native COMPATF_NOBLOCKFRIENDS: friendly actors bypass monster-blocking line flags.</summary>
    NoBlockFriends = 16384,
    /// <summary>Native COMPATF2_RAILING: railing floors apply only from the high side.</summary>
    Railing = 32768,
    /// <summary>Native COMPATF_NO_PASSMOBJ: disables actor-top support and stepping.</summary>
    NoPassMobj = 65536,
    /// <summary>Native COMPATF_SOUNDTARGET: look reads sector noise rather than per-actor LastHeard.</summary>
    SoundTarget = 131072,
    /// <summary>Native COMPATF_MBFMONSTERMOVE: terrain friction affects monster walking.</summary>
    MbfMonsterMove = 262144,
    /// <summary>Native COMPATF_DROPOFF: disables chase escape-vector generation at drop-offs.</summary>
    DropOff = 524288,
    /// <summary>Native COMPATF2_AVOID_HAZARDS: avoid entering damaging sectors and crushers.</summary>
    AvoidHazards = 1048576,
    /// <summary>Native COMPATF2_STAYONLIFT: encourage actors to remain on a target's tagged lift.</summary>
    StayOnLift = 2097152,
    /// <summary>Native BCOMPATF_LINKFROZENPROPS: all-player freeze actions update both modes.</summary>
    LinkFrozenProperties = 4194304,
}

public static class CompatSurfaceRules
{
    public const int EternityReverbSpecial = 200;
    public const int Id24SecretExit = 243;
    public const int Mbf21FloorNudge = 270;
}

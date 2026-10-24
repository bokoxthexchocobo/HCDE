namespace HCDE.Gamedata;

/// <summary>MBF21 <c>deh_mobjflags_mbf21</c> identifiers from <c>d_dehacked.cpp</c>.</summary>
public static class VmDehackedMbf21FlagNames
{
    public static IReadOnlyList<string> KnownFlags =>
    [
        "LOGRAV", "SHORTMRANGE", "DMGIGNORED", "NORADIUSDMG", "FORCERADIUSDMG",
        "HIGHERMPROB", "RANGEHALF", "NOTHRESHOLD", "LONGMELEE", "BOSS",
        "MAP07BOSS1", "MAP07BOSS2", "E1M8BOSS", "E2M8BOSS", "E3M8BOSS",
        "E4M6BOSS", "E4M8BOSS", "RIP", "FULLVOLSOUNDS"
    ];

    public static void ClearMbf21Defaults(VmDehackedMbf21ActorDefaults defaults)
    {
        defaults.Gravity = 1;
        defaults.MaxTargetRange = 0;
        defaults.MinMissileChance = 200;
        defaults.MeleeThreshold = 0;
        defaults.MissileChanceMult = 1;
        defaults.IsBoss = false;
        defaults.Rip = false;
        defaults.NoTarget = false;
        defaults.NoRadiusDamage = false;
        defaults.QuickToRetaliate = false;
        defaults.ForceRadiusDamage = false;
    }
}

public sealed class VmDehackedMbf21ActorDefaults
{
    public double Gravity { get; set; } = 1;
    public int MaxTargetRange { get; set; }
    public int MinMissileChance { get; set; } = 200;
    public int MeleeThreshold { get; set; }
    public double MissileChanceMult { get; set; } = 1;
    public bool IsBoss { get; set; }
    public bool Rip { get; set; }
    public bool NoTarget { get; set; }
    public bool NoRadiusDamage { get; set; }
    public bool QuickToRetaliate { get; set; }
    public bool ForceRadiusDamage { get; set; }
}

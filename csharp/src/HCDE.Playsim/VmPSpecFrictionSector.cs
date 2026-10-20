namespace HCDE.Playsim;

/// <summary>Sector damage entry, timelimit, and friction from <c>p_spec.cpp</c> (406–477, 685–816).</summary>
public static class VmPSpecFrictionSector
{
    public const double OrigFriction = 0xE800 / 65536.0;
    public const int SecfExit1 = 1;
    public const int SecfExit2 = 2;
    public const int SecfEndgodmode = 4;
    public const int SecfHazard = 8;
    public const int SecfEndlevel = 16;
    public const int SecfDmterrainfx = 32;
    public const int SecfFriction = 64;
    public const int DfNoExit = 1;
    public const int Ticrate = 35;

    public static bool TimelimitExit(bool deathmatch, double timelimitMinutes, int maptimeTics) =>
        deathmatch && timelimitMinutes > 0 && maptimeTics >= (int)(timelimitMinutes * Ticrate * 60);

    public static bool SectorExitKillsPlayers(int flags) => (flags & (SecfExit1 | SecfExit2)) != 0;

    public static bool UseSecretExit(int flags) => (flags & SecfExit2) != 0;

    public static int DefaultDamageInterval(int interval) => interval <= 0 ? 32 : interval;

    public static int IronFeetProtectionLevel(bool full, bool normal, int current) =>
        full ? 2 : normal && current < 1 ? 1 : current;

    public static bool HazardUsesAccumulator(bool hazard, object? player) => hazard && player != null;

    public static bool DamageTickDue(int levelTime, int interval) => interval != 0 && levelTime % interval == 0;

    public static bool EndlevelExitOnLowHealth(bool endlevel, int health, bool deathmatch, int dmflags) =>
        endlevel && health <= 10 && (!deathmatch || (dmflags & DfNoExit) == 0);

    public static bool HealOnNegativeDamageTick(int damageAmount, int levelTime, int interval) =>
        damageAmount < 0 && levelTime % interval == 0;

    public static double SectorFrictionFromAmount(int amount) =>
        Math.Clamp(((0x1EB8 * amount) / 0x80 + 0xD001) / 65536.0, 0, 1);

    public static double FrictionToMoveFactorIce(double friction) =>
        (((0x10092 - friction * 65536) * 1024) / 4352 + 568) / 65536.0;

    public static double FrictionToMoveFactorMud(double friction) =>
        (((friction * 65536.0 - 0xDB34) * 0xA) / 0x80) / 65536.0;

    public static double FrictionToMoveFactor(double friction) =>
        friction >= OrigFriction
            ? FrictionToMoveFactorIce(friction)
            : FrictionToMoveFactorMud(friction);

    public static double ClampMoveFactor(double movefactor) =>
        movefactor < 1 / 2048.0 ? 1 / 2048.0 : movefactor;

    public static bool ClearFrictionFlagWhenOrig(double friction) => friction == OrigFriction;

    public static bool SetFrictionFlagWhenCustom(double friction) => friction != OrigFriction;

    public static bool SkipUntilGround(bool evilAir, bool hasFfloor, bool ffloorSolid, int waterlevel, bool atFloorZ) =>
        (!evilAir && !(hasFfloor && !ffloorSolid) && waterlevel == 0) && !atFloorZ;

    public static bool IronFeetLeakDamage(int ironfeet, int leakRoll, int leakyDamage) =>
        ironfeet == 0 || (ironfeet < 2 && leakRoll < leakyDamage);

    public static bool StripGodmodeOnSector(bool player, int sectorFlags) =>
        player && (sectorFlags & SecfEndgodmode) != 0;

    public static bool UseHazardAccumulator(bool player, int sectorFlags) =>
        player && (sectorFlags & SecfHazard) != 0;

    public static bool SkipDamageWhenGodmode(bool player, int cheats) =>
        player && (cheats & (1 | 2)) != 0;

    public static bool TriggerTerrainFxOnDamage(int sectorFlags) => (sectorFlags & SecfDmterrainfx) != 0;

    public static bool NegativeDamageHealTick(int damageAmount, int levelTime, int interval) =>
        damageAmount < 0 && levelTime % interval == 0;

    public static int FrictionFlagAfterSet(double friction, int flags) =>
        friction == OrigFriction ? flags & ~SecfFriction : flags | SecfFriction;

    public static (double Friction, double MoveFactor) SectorFrictionPair(int amount) =>
        (SectorFrictionFromAmount(amount), ClampMoveFactor(FrictionToMoveFactor(SectorFrictionFromAmount(amount))));
}

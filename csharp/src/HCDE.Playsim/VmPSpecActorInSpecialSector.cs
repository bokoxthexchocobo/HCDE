namespace HCDE.Playsim;

/// <summary>
/// `P_ActorInSpecialSector`, `P_UpdateSpecials`, `P_SetSectorFriction`, and `FrictionToMoveFactor` from
/// <c>p_spec.cpp</c> (406–501, 685–696, 751–816).
/// </summary>
public static class VmPSpecActorInSpecialSector
{
    public const int SecmfHarinair = 1;
    public const int FfSolid = 1;
    public const int SecfExit1 = 1;
    public const int SecfExit2 = 2;
    public const int SecfEndgodmode = 4;
    public const int SecfHazard = 8;
    public const int SecfEndlevel = 16;
    public const int SecfDmterrainfx = 32;
    public const int SecfFriction = 64;
    public const int CfGodmode = 1;
    public const int CfGodmode2 = 2;
    public const int DfNoExit = 1;
    public const int TelefragDamage = 1000000;
    public const double OrigFriction = 0xE800 / 65536.0;
    public const int Ticrate = 35;

    public sealed class Sector
    {
        public int MoreFlags { get; set; }
        public int Flags { get; set; }
        public int DamageInterval { get; set; }
        public int DamageAmount { get; set; }
        public int LeakyDamage { get; set; }
        public object? DamageType { get; set; }
        public bool Secret { get; set; }
        public int Index { get; set; }
        public double Friction { get; set; }
        public double MoveFactor { get; set; }
    }

    public sealed class Player
    {
        public int Cheats { get; set; }
        public int Health { get; set; }
        public int HazardCount { get; set; }
        public object? HazardType { get; set; }
        public int HazardInterval { get; set; }
    }

    public sealed class Actor
    {
        public int WaterLevel { get; set; }
        public Player? Player { get; set; }
        public IEnumerable<object>? Inventory { get; set; }
    }

    public sealed class F3DFloor
    {
        public int Flags { get; set; }
        public Func<double>? TopZAtActor { get; init; }
    }

    public sealed class ActorContext
    {
        public Func<Actor, double>? LowestFloorZ { get; init; }
        public Func<Actor, double, bool>? IsAtZ { get; init; }
        public Func<int>? IronFeetLevel { get; init; }
        public Func<int>? LeakRoll { get; init; }
        public Action? KillAllPlayers { get; init; }
        public Action<bool>? ExitLevel { get; init; }
        public Action<int, object?>? DamageActor { get; init; }
        public Action<int>? HealActor { get; init; }
        public Action? HitWaterTerrainFx { get; init; }
        public Action? ClearSecret { get; init; }
        public Action<int>? GiveSecret { get; init; }
        public int LevelTime { get; init; }
        public bool Deathmatch { get; init; }
        public int Dmflags { get; init; }
    }

    public static bool ShouldReturnBeforeGround(
        bool evilAir, bool hasFfloor, bool ffloorSolid, int waterlevel, bool atFloorZ) =>
        (!evilAir && !(hasFfloor && !ffloorSolid) && waterlevel == 0) && !atFloorZ;

    public static void ActorInSpecialSector(Actor victim, Sector sector, F3DFloor? ffloor, ActorContext ctx)
    {
        var evilAir = (sector.MoreFlags & SecmfHarinair) != 0;
        var solidFfloor = ffloor != null && (ffloor.Flags & FfSolid) != 0;
        var theZ = ffloor == null
            ? ctx.LowestFloorZ?.Invoke(victim) ?? 0
            : ffloor.TopZAtActor?.Invoke() ?? 0;
        var atZ = ctx.IsAtZ?.Invoke(victim, theZ) == true;
        if (ShouldReturnBeforeGround(evilAir, ffloor != null, solidFfloor, victim.WaterLevel, atZ))
            return;

        if (sector.DamageInterval <= 0)
            sector.DamageInterval = 32;

        if (victim.Player != null && (sector.Flags & (SecfExit1 | SecfExit2)) != 0)
        {
            ctx.KillAllPlayers?.Invoke();
            ctx.ExitLevel?.Invoke((sector.Flags & SecfExit2) != 0);
            return;
        }

        if (sector.DamageAmount > 0)
        {
            var ironfeet = ctx.IronFeetLevel?.Invoke() ?? 0;
            if (victim.Player != null && (sector.Flags & SecfEndgodmode) != 0)
                victim.Player.Cheats &= ~CfGodmode;
            var leak = ctx.LeakRoll?.Invoke() ?? 256;
            if (ironfeet == 0 || (ironfeet < 2 && leak < sector.LeakyDamage))
            {
                if (victim.Player != null && (sector.Flags & SecfHazard) != 0)
                {
                    victim.Player.HazardCount += sector.DamageAmount;
                    victim.Player.HazardType = sector.DamageType;
                    victim.Player.HazardInterval = sector.DamageInterval;
                }
                else if (sector.DamageInterval != 0 && ctx.LevelTime % sector.DamageInterval == 0)
                {
                    if (victim.Player == null || (victim.Player.Cheats & (CfGodmode | CfGodmode2)) == 0)
                        ctx.DamageActor?.Invoke(sector.DamageAmount, sector.DamageType);
                    if (victim.Player != null && (sector.Flags & SecfEndlevel) != 0 &&
                        victim.Player.Health <= 10 && (!ctx.Deathmatch || (ctx.Dmflags & DfNoExit) == 0))
                        ctx.ExitLevel?.Invoke(false);
                    if ((sector.Flags & SecfDmterrainfx) != 0)
                        ctx.HitWaterTerrainFx?.Invoke();
                }
            }
        }
        else if (sector.DamageAmount < 0 && sector.DamageInterval != 0 && ctx.LevelTime % sector.DamageInterval == 0)
            ctx.HealActor?.Invoke(-sector.DamageAmount);

        if (victim.Player != null && sector.Secret)
        {
            ctx.ClearSecret?.Invoke();
            ctx.GiveSecret?.Invoke(sector.Index);
        }
    }

    public static bool UpdateSpecialsTimelimit(bool deathmatch, double timelimitMinutes, int maptimeTics) =>
        deathmatch && timelimitMinutes > 0 && maptimeTics >= (int)(timelimitMinutes * Ticrate * 60);

    public static double SectorFrictionFromAmount(int amount) =>
        Math.Clamp(((0x1EB8 * amount) / 0x80 + 0xD001) / 65536.0, 0, 1);

    public static double FrictionToMoveFactor(double friction)
    {
        double movefactor = friction >= OrigFriction
            ? (((0x10092 - friction * 65536) * 1024) / 4352 + 568) / 65536.0
            : (((friction * 65536.0 - 0xDB34) * 0xA) / 0x80) / 65536.0;
        if (movefactor < 1 / 2048.0)
            movefactor = 1 / 2048.0;
        return movefactor;
    }

    public static void ApplySectorFriction(Sector sector, int amount, bool alterFlag)
    {
        var friction = SectorFrictionFromAmount(amount);
        var movefactor = FrictionToMoveFactor(friction);
        sector.Friction = friction;
        sector.MoveFactor = movefactor;
        if (!alterFlag)
            return;
        sector.Flags = friction == OrigFriction
            ? sector.Flags & ~SecfFriction
            : sector.Flags | SecfFriction;
    }
}

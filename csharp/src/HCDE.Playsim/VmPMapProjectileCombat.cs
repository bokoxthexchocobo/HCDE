namespace HCDE.Playsim;

/// <summary>Projectile immunity, infight gating, and missile damage from <c>p_map.cpp</c> (1227–1359).</summary>
public static class VmPMapProjectileCombat
{
    public sealed class SpechitEntry
    {
        public object? Line { get; init; }
        public (double X, double Y) Refpos { get; init; }
        public (double X, double Y) Oldrefpos { get; init; }
    }

    public static void RecordSpecialAndPortalHits(
        object? line,
        int special,
        bool isPortal,
        (double X, double Y) refpos,
        (double X, double Y) oldrefpos,
        List<SpechitEntry> spechit,
        List<SpechitEntry> portalhit)
    {
        if (ShouldRecordSpechit(special))
            spechit.Add(new SpechitEntry { Line = line, Refpos = refpos, Oldrefpos = oldrefpos });
        if (ShouldRecordPortalHit(isPortal))
            portalhit.Add(new SpechitEntry { Line = line, Refpos = refpos, Oldrefpos = oldrefpos });
    }

    public static bool CheckRipLevel(int victimMin, int victimMax, int projectileLevel)
    {
        if (victimMin > 0 && projectileLevel < victimMin)
            return false;
        if (victimMax > 0 && projectileLevel > victimMax)
            return false;
        return true;
    }

    public static bool ShouldRecordSpechit(int special) => special != 0;

    public static bool ShouldRecordPortalHit(bool isPortal) => isPortal;

    public static bool ShouldAdjustPortalCeiling(double openTop, double zofs, double ceilingz) =>
        openTop - zofs < ceilingz;

    public static bool ShouldAdjustPortalFloor(double openBottom, double zofs, double floorz) =>
        openBottom - zofs > floorz;

    public static bool ShouldLowerPortalDropoff(double openLowFloor, double zofs, double dropoffz) =>
        openLowFloor - zofs < dropoffz;

    public static int MissileDamageSides(bool ripper, int flags4Strife) =>
        ripper ? 3 : (flags4Strife != 0 ? 3 : 7);

    public static int MissileDamageCount(bool ripper) => ripper ? 2 : 1;

    public static bool ShouldApplyMissileDamage(int damage, int flags6ForcePain, int flags7CausePain) =>
        damage > 0 || flags6ForcePain != 0 || flags7CausePain != 0;

    public static bool ShouldPoisonOnHit(int poisonDamage, int poisonDuration) =>
        poisonDamage > 0 && poisonDuration != int.MinValue;

    public sealed class PortalProbeContext
    {
        public required Func<object, IEnumerable<LineStub>> LinesAt { get; init; }
        public required Func<LineStub, (double X, double Y), (double Top, double Bottom)> LineOpening { get; init; }
        public required Action<double> AddZ { get; init; }
    }

    public sealed class LineStub
    {
        public object? BackSector { get; set; }
        public object? FrontSector { get; init; }
    }

    public static bool CheckPortalOpening(
        LineStub portalLine,
        double zofs,
        ref double ceilingz,
        ref double floorz,
        ref double dropoffz,
        (double X, double Y) pos,
        PortalProbeContext ctx)
    {
        var lp = portalLine;
        var sec = lp.BackSector;
        if (lp.BackSector == null)
            lp.BackSector = lp.FrontSector;
        ctx.AddZ(zofs);
        var ret = false;
        foreach (var ld in ctx.LinesAt(portalLine))
        {
            if (ld.BackSector == null)
                continue;
            var open = ctx.LineOpening(ld, pos);
            if (ShouldAdjustPortalCeiling(open.Top, zofs, ceilingz))
            {
                ceilingz = open.Top - zofs;
                ret = true;
            }
            if (ShouldAdjustPortalFloor(open.Bottom, zofs, floorz))
            {
                floorz = open.Bottom - zofs;
                ret = true;
            }
            if (ShouldLowerPortalDropoff(open.Bottom, zofs, dropoffz))
                dropoffz = open.Bottom - zofs;
        }
        ctx.AddZ(-zofs);
        lp.BackSector = sec;
        return ret;
    }

    public const int PgGroupless = -1;
    public const int PgDefault = 0;

    public sealed class CombatActor
    {
        public CombatActor? Self { get; init; }
        public int ProjectileGroup { get; init; }
        public string Species { get; init; } = "";
        public int Flags6DoHarmSpecies { get; init; }
        public bool Player { get; init; }
        public int Tid { get; init; }
        public int TidToHate { get; init; }
        public int Flags3Monster { get; init; }
        public int FlagsShootable { get; init; }
        public int Flags7ForceInfighting { get; init; }
        public int Flags7HarmFriends { get; init; }
        public bool HostileToShooter { get; init; }
        public bool FriendOfShooter { get; init; }
    }

    public static bool ProjectileImmune(CombatActor target, CombatActor source)
    {
        var targetgroup = target.ProjectileGroup;
        var sourcegroup = source.ProjectileGroup;
        return (targetgroup != PgGroupless || ReferenceEquals(target.Self, source.Self))
            && (
                (targetgroup == PgDefault
                    && source.Species == target.Species
                    && target.Flags6DoHarmSpecies == 0)
                || (targetgroup != PgDefault && targetgroup == sourcegroup));
    }

    public sealed class InfightContext
    {
        public int LevelInfighting { get; init; }
    }

    public static bool CanAttackHurt(CombatActor victim, CombatActor shooter, InfightContext ctx)
    {
        if (!victim.Player && !shooter.Player)
        {
            var infight = ctx.LevelInfighting;
            if (infight < 0 && victim.Flags7ForceInfighting != 0)
                infight = 0;
            if (infight < 0)
            {
                if (shooter.FlagsShootable != 0)
                {
                    if (victim.Flags3Monster != 0)
                    {
                        if (!victim.HostileToShooter)
                        {
                            if (victim.Tid == 0 || shooter.TidToHate != victim.Tid)
                                return false;
                        }
                    }
                }
            }
            else if (infight == 0)
            {
                if (victim.FriendOfShooter)
                {
                    if (shooter.Flags7HarmFriends == 0)
                        return false;
                }
                else
                {
                    if (victim.TidToHate != 0 && victim.TidToHate == shooter.TidToHate)
                        return false;
                    if (ProjectileImmune(victim, shooter) && !victim.HostileToShooter)
                    {
                        if (victim.Tid == 0 || shooter.TidToHate != victim.Tid)
                            return false;
                    }
                }
            }
        }
        return true;
    }

    public sealed class MissileDamageContext
    {
        public int PoisonDamage { get; init; }
        public int PoisonDuration { get; init; }
        public bool Ripper { get; init; }
        public required Func<int, int, int> MissileDamage { get; init; }
        public required Action<CombatActor, CombatActor, int, string> DamageMobj { get; init; }
        public required Action<CombatActor, CombatActor> PoisonMobj { get; init; }
        public required Action<CombatActor, int> GiveBody { get; init; }
        public int Flags4Strife { get; init; }
        public int Flags6ForcePain { get; init; }
        public int Flags7CausePain { get; init; }
    }

    public static void DoMissileDamage(CombatActor self, CombatActor victim, MissileDamageContext ctx)
    {
        if (ShouldPoisonOnHit(ctx.PoisonDamage, ctx.PoisonDuration))
            ctx.PoisonMobj(victim, self);
        var ripper = ctx.Ripper;
        var damage = ctx.MissileDamage(MissileDamageSides(ripper, ctx.Flags4Strife), MissileDamageCount(ripper));
        if (ShouldApplyMissileDamage(damage, ctx.Flags6ForcePain, ctx.Flags7CausePain))
            ctx.DamageMobj(victim, self, damage, "Unknown");
        else
            ctx.GiveBody(victim, -damage);
    }
}

namespace HCDE.Playsim;

/// <summary>
/// Hitscan <c>P_LineAttack</c> from <c>p_map.cpp</c> (<b>4691–5050</b>). Chunk <b>1169</b>.
/// </summary>
public static class VmPMapLineAttack1169Slice
{
    public const int LafIsMeleeAttack = 1;
    public const int LafNoRandomPuffZ = 1 << 1;
    public const int LafNoImpactDecal = 1 << 2;
    public const int LafNoInteract = 1 << 3;
    public const int LafTargetIsSource = 1 << 4;
    public const int LafOverrideZ = 1 << 5;
    public const int LafAbsOffset = 1 << 6;
    public const int LafAbsPosition = 1 << 7;

    public const int PfHitThing = 1;
    public const int PfMeleeRange = 2;
    public const int PfTemporary = 4;
    public const int PfHitThingBleed = 8;
    public const int PfNoRandomZ = 16;
    public const int PfHitSky = 32;

    public const int TraceHitFloor = 1;
    public const int TraceHitWall = 3;
    public const int TraceHitActor = 4;
    public const int TraceHasHitSky = 6;
    public const int TraceNoSky = 0x0001;
    public const int TraceImpact = 0x0004;
    public const int TraceHitSky = 0x0040;

    public const int MfShootable = 0x00000004;
    public const int MfNoBlood = 0x00080000;
    public const int MlBlockEverything = 0x00008000;
    public const int MlBlockHitscan = 0x08000000;
    public const int LineHorizon = 9;

    public const int Mf2ThruGhost = 0x00004000;
    public const int Mf2ThruActors = 0x00100000;
    public const int Mf2Invulnerable = 0x08000000;
    public const int Mf2Dormant = 0x10000000;
    public const int Mf3AlwaysPuff = 0x00000010;
    public const int Mf3SkyExplode = 0x00004000;
    public const int Mf3BloodlessImpact = 0x08000000;
    public const int Mf3PuffOnActors = 0x40000000;
    public const int Mf4Spectral = 0x00040000;
    public const int Mf5PierceArmor = 0x00000800;
    public const int Mf6ThruSpecies = 0x00000002;
    public const int Mf6MThruSpecies = 0x00000004;
    public const int Mf6ForcePain = 0x00000008;
    public const int Mf6NoTrigger = 0x00010000;
    public const int Mf7CausePain = 0x00000400;
    public const int Mf7NoDecal = 0x00040000;
    public const int Mf7ForceDecal = 0x00080000;
    public const int Mf7AllowThruFlags = 0x00400000;
    public const int Mf8AllowThruBits = 0x00008000;

    public const int DmgNoArmor = 1;
    public const int DmgInflictorIsPuff = 2;
    public const int DmgPlayerAttack = 32;
    public const int DmgUseAngle = 512;
    public const int CompatHitscan = 1 << 28;
    public const int AllowReplace = 1;
    public const int ChanWeapon = 1;
    public const double AttnNorm = 1d;
    public const double EqualEpsilon = 1d / 65536d;

    public readonly record struct Vec3(double X, double Y, double Z);

    public sealed class Shooter
    {
        public double X = 100;
        public double Y = 200;
        public double Z = 8;
        public double Center = 30;
        public double Floorclip = 2;
        public double AttackOffset = 1;
        public bool Player;
        public bool WeaponThruGhost;
        public bool WeaponPierceArmor;
        public bool WeaponDecal;
        public bool Decal;
        public Shooter? Target;
        public object? Sector = "sector";
        public double AngleToDegrees = 12;
    }

    public sealed class PuffDefaults
    {
        public int Flags2;
        public int Flags3;
        public int Flags4;
        public int Flags6;
        public int Flags7;
        public string DamageType = "None";
        public bool ActiveSoundValid;
        public string ActiveSound = "";
        public int PoisonDamage;
        public int PoisonDuration;
        public int PoisonPeriod;
        public string PoisonDamageType = "None";
    }

    public sealed class PuffActor
    {
        public double X;
        public double Y;
        public double Z;
        public double Radius;
        public bool HasDecal;
        public bool Destroyed;
        public string Species = "None";
        public int Flags8;
        public int ThruBits;
    }

    public sealed class LineTarget
    {
        public object? Target;
        public double AttackAngle;
        public double AngleFromSource;
        public bool Unlinked;

        public void Clear()
        {
            Target = null;
            AttackAngle = 0;
            AngleFromSource = 0;
            Unlinked = false;
        }
    }

    public sealed class DamageSlot
    {
        public int Value;
    }

    public sealed class OriginInfo
    {
        public bool HitGhosts;
        public bool MThruSpecies;
        public string PuffSpecies = "None";
        public bool Spectral;
        public bool ThruSpecies;
        public bool ThruActors;
        public bool UseThruBits;
        public int ThruBits;
    }

    public sealed class TraceQuery
    {
        public Vec3 From;
        public Vec3 Direction;
        public double Distance;
        public int ActorMask;
        public int WallMask;
        public int TraceFlags;
        public OriginInfo Origin = new();
    }

    public sealed class TraceHit
    {
        public int HitType;
        public Vec3 HitPos;
        public Vec3 HitVector;
        public double SrcAngle;
        public bool Unlinked;
        public int LineSpecial;
        public bool VisualPortal;
        public bool CrossedWater;
        public bool Crossed3DWater;
        public bool HeightSec;
        public int ActorFlags;
        public int ActorFlags2;
        public int ActorCompat;
        public object Actor = "victim";
    }

    public sealed class SpawnCall
    {
        public Vec3 Pos;
        public double HitDir;
        public double ParticleDir;
        public int UpDown;
        public int Flags;
        public object? Victim;
        public int ReplaceMode;
        public PuffActor Actor = new();
    }

    public sealed class AttackHost
    {
        public bool PreFired;
        public bool Suppress;
        public PuffDefaults Defaults = new();
        public bool SpawnHasDecal;
        public bool SpawnSpeciesProbe = true;
        public int SpeciesReplace;
        public string ProbeSpecies = "Imp";
        public int ProbeFlags8;
        public int ProbeThruBits = 5;
        public bool DidHit;
        public TraceHit Trace = new();
        public int DamageResult = 4;
        public List<string> Lag { get; } = new();
        public List<TraceQuery> Traces { get; } = new();
        public List<SpawnCall> Spawns { get; } = new();
        public List<PuffActor> DestroyedProbes { get; } = new();
        public string? Sound;
        public string ResolvedDamageType = "";
        public bool Geometry;
        public int GeometryDamage;
        public string GeometryType = "";
        public List<(string Who, string Source)> Decals { get; } = new();
        public bool Water;
        public int PoisonDamage = -1;
        public object? PoisonInflictor;
        public int AppliedFlags;
        public object? AppliedSource;
        public object? AppliedInflictor;
        public double AppliedAngle;
        public bool Blood;
        public double BloodX;
        public double BloodDamage;
        public double BloodNew;
        public double BleedAmount = -1;
        public Vec3 BleedAt;
        public bool Splash;
        public PuffActor? SplashPuff;
        public bool Fired;
        public Vec3 FiredFrom;
        public Vec3 FiredPuffPos;
        public PuffActor? FiredPuff;
        public int FiredFlags;
        public bool Cosmetic;
        public double CosmeticAngle;

        public Vec3 OffsetZ(double x, double y, double z)
        {
            return new Vec3(XOrigin + x, YOrigin + y, z);
        }

        public Vec3 PosAtZ(double z)
        {
            return new Vec3(XOrigin, YOrigin, z);
        }

        public (double X, double Y) Portal(double x, double y, double dx, double dy)
        {
            return (x + dx, y + dy);
        }

        public double XOrigin = 100;
        public double YOrigin = 200;
    }

    public static PuffActor? LineAttack(
        Shooter shooter,
        double angle,
        double distance,
        double pitch,
        int damage,
        string damageType,
        string puffType,
        int flags,
        LineTarget? victim,
        DamageSlot? actualDamage,
        double sz,
        double offsetForward,
        double offsetSide,
        AttackHost host)
    {
        if (host.PreFired)
        {
            return null;
        }

        if (host.Suppress)
        {
            victim?.Clear();
            if (actualDamage != null)
            {
                actualDamage.Value = 0;
            }

            return null;
        }

        host.Lag.Add("lineattack");
        var noInteract = (flags & LafNoInteract) != 0;
        var puffFlags = (flags & LafIsMeleeAttack) != 0 ? PfMeleeRange : 0;
        if ((flags & LafNoRandomPuffZ) != 0)
        {
            puffFlags |= PfNoRandomZ;
        }

        victim?.Clear();
        if (actualDamage != null)
        {
            actualDamage.Value = 0;
        }

        var pitchCos = CosDeg(pitch);
        var direction = new Vec3(pitchCos * CosDeg(angle), pitchCos * SinDeg(angle), -SinDeg(pitch));
        var shootZ = shooter.Center - shooter.Floorclip + shooter.AttackOffset;
        var playerAttack = 0;
        if (shooter.Player)
        {
            playerAttack = DmgPlayerAttack;
        }

        if ((flags & LafOverrideZ) != 0)
        {
            shootZ = shooter.Z;
        }

        shootZ += sz;
        var defaults = host.Defaults;
        var origin = new OriginInfo
        {
            HitGhosts = (shooter.Player && shooter.WeaponThruGhost) || (defaults.Flags2 & Mf2ThruGhost) != 0,
            MThruSpecies = (defaults.Flags6 & Mf6MThruSpecies) != 0,
            Spectral = (defaults.Flags4 & Mf4Spectral) != 0,
        };
        var spawnSky = (defaults.Flags3 & Mf3SkyExplode) != 0;
            if ((defaults.Flags7 & Mf7AllowThruFlags) != 0)
            {
                origin.ThruSpecies = (defaults.Flags6 & Mf6ThruSpecies) != 0;
                origin.ThruActors = (defaults.Flags2 & Mf2ThruActors) != 0;
                if (puffType.Length > 0)
                {
                    host.SpeciesReplace = AllowReplace;
                var probe = host.SpawnSpeciesProbe
                    ? new PuffActor
                    {
                        Species = host.ProbeSpecies,
                        Flags8 = host.ProbeFlags8,
                        ThruBits = host.ProbeThruBits,
                    }
                    : null;
                if (probe != null)
                {
                    origin.PuffSpecies = probe.Species;
                    origin.UseThruBits = (probe.Flags8 & Mf8AllowThruBits) != 0;
                    origin.ThruBits = probe.ThruBits;
                    probe.Destroyed = true;
                    host.DestroyedProbes.Add(probe);
                }
            }
        }

        if ((damageType == "None" || damageType == "Melee" || damageType == "Hitscan")
            && defaults.DamageType != "None")
        {
            damageType = defaults.DamageType;
        }

        host.ResolvedDamageType = damageType;
        var traceFlags = TraceNoSky | TraceImpact;
        if (noInteract || (defaults.Flags6 & Mf6NoTrigger) != 0)
        {
            traceFlags &= ~TraceImpact;
        }

        if (spawnSky)
        {
            traceFlags &= ~TraceNoSky;
            traceFlags |= TraceHitSky;
        }

        Vec3 tempos;
        if ((flags & LafAbsPosition) != 0)
        {
            tempos = new Vec3(offsetForward, offsetSide, sz);
        }
        else if ((flags & LafAbsOffset) != 0)
        {
            tempos = host.OffsetZ(offsetForward, offsetSide, shootZ);
        }
        else if (offsetForward == 0d && offsetSide == 0d)
        {
            tempos = host.PosAtZ(shootZ);
        }
        else
        {
            var s = SinDeg(angle);
            var c = CosDeg(angle);
            tempos = host.OffsetZ((offsetForward * c) + (offsetSide * s), (offsetForward * s) - (offsetSide * c), shootZ);
        }

        var query = new TraceQuery
        {
            From = tempos,
            Direction = direction,
            Distance = distance,
            ActorMask = MfShootable,
            WallMask = MlBlockEverything | MlBlockHitscan,
            TraceFlags = traceFlags,
            Origin = origin,
        };
        host.Traces.Add(query);
        var hit = host.Trace;
        PuffActor? puff = null;
        var killPuff = false;
        var puffPos = new Vec3();
        if (!host.DidHit)
        {
            if (!noInteract && defaults.ActiveSoundValid)
            {
                host.Sound = defaults.ActiveSound;
            }

            if (noInteract || (defaults.Flags3 & Mf3AlwaysPuff) != 0)
            {
                puffPos = hit.HitPos;
                puff = Spawn(host, puffPos, hit.SrcAngle, hit.SrcAngle, 2, puffFlags, null);
                if (noInteract)
                {
                    return puff;
                }
            }
            else
            {
                return null;
            }
        }
        else if (hit.HitType != TraceHitActor)
        {
            if (hit.HitType == TraceHasHitSky
                || (hit.HitType == TraceHitWall && hit.LineSpecial == LineHorizon && spawnSky))
            {
                puffFlags |= PfHitSky;
            }

            host.Geometry = true;
            host.GeometryDamage = damage;
            host.GeometryType = damageType;
            if (victim != null)
            {
                victim.Unlinked = hit.Unlinked;
            }

            if (noInteract || hit.HitType != TraceHitWall || hit.LineSpecial != LineHorizon || spawnSky)
            {
                var pulled = host.Portal(hit.HitPos.X, hit.HitPos.Y, -hit.HitVector.X * 4d, -hit.HitVector.Y * 4d);
                puffPos = new Vec3(pulled.X, pulled.Y, hit.HitPos.Z - (hit.HitVector.Z * 4d));
                puff = Spawn(host, puffPos, hit.SrcAngle, hit.SrcAngle - 90d, 0, puffFlags, null);
                puff.Radius = EqualEpsilon;
                if (noInteract)
                {
                    return puff;
                }
            }

            if (hit.HitType == TraceHitWall
                && hit.LineSpecial != LineHorizon
                && !hit.VisualPortal
                && (flags & LafNoImpactDecal) == 0
                && (defaults.Flags7 & Mf7NoDecal) == 0)
            {
                if (shooter.Decal || (shooter.Player && shooter.WeaponDecal))
                {
                    if ((defaults.Flags7 & Mf7ForceDecal) != 0 && puff != null && puff.HasDecal)
                    {
                        host.Decals.Add(("puff", "puff"));
                    }
                    else
                    {
                        host.Decals.Add(("shooter", "shooter"));
                    }
                }
                else if (puff != null && puff.HasDecal)
                {
                    host.Decals.Add(("puff", "puff"));
                }
                else
                {
                    host.Decals.Add(("shooter", "shooter"));
                }
            }
            else if (puff != null && !hit.CrossedWater && !hit.HeightSec && hit.HitType == TraceHitFloor)
            {
                host.Water = true;
            }
        }
        else
        {
            var bleed = hit.HitPos;
            puffPos = bleed;
            if ((hit.ActorCompat & CompatHitscan) != 0)
            {
                var shifted = host.Portal(bleed.X, bleed.Y, -10d * hit.HitVector.X, -10d * hit.HitVector.Y);
                bleed = new Vec3(shifted.X, shifted.Y, bleed.Z - (-10d * hit.HitVector.Z));
            }

            if (noInteract
                || (defaults.Flags3 & Mf3PuffOnActors) != 0
                || (hit.ActorFlags & MfNoBlood) != 0
                || (hit.ActorFlags2 & (Mf2Invulnerable | Mf2Dormant)) != 0)
            {
                if ((hit.ActorFlags & MfNoBlood) == 0)
                {
                    puffFlags |= PfHitThingBleed;
                }

                puff = Spawn(host, bleed, hit.SrcAngle, hit.SrcAngle - 90d, 2, puffFlags | PfHitThing, hit.Actor);
            }

            if (victim != null)
            {
                victim.Target = hit.Actor;
                victim.AttackAngle = hit.SrcAngle;
                victim.AngleFromSource = hit.Unlinked ? hit.SrcAngle : shooter.AngleToDegrees;
                victim.Unlinked = hit.Unlinked;
            }

            if (noInteract)
            {
                return puff;
            }

            if (defaults.PoisonDamage > 0 && defaults.PoisonDuration != int.MinValue)
            {
                host.PoisonDamage = defaults.PoisonDamage;
                host.PoisonInflictor = puff ?? (object)shooter;
            }

            var newDamage = damage;
            if (damage != 0 || (defaults.Flags6 & Mf6ForcePain) != 0 || (defaults.Flags7 & Mf7CausePain) != 0)
            {
                var damageFlags = DmgInflictorIsPuff | playerAttack;
                if (shooter.Player && (damageFlags & DmgPlayerAttack) != 0 && shooter.WeaponPierceArmor)
                {
                    damageFlags |= DmgNoArmor;
                }

                if (puff == null)
                {
                    puff = Spawn(host, bleed, 0, 0, 2, puffFlags | PfHitThing | PfTemporary, null);
                    killPuff = true;
                }

                var source = shooter;
                if ((flags & LafTargetIsSource) != 0 && shooter.Target != null)
                {
                    source = shooter.Target;
                }

                newDamage = host.DamageResult;
                host.AppliedFlags = damageFlags | DmgUseAngle;
                host.AppliedSource = source;
                host.AppliedInflictor = puff ?? (object)shooter;
                host.AppliedAngle = hit.SrcAngle;
                if (actualDamage != null)
                {
                    actualDamage.Value = newDamage;
                }
            }

            if ((defaults.Flags3 & Mf3BloodlessImpact) == 0)
            {
                host.Blood = true;
                host.BloodX = bleed.X;
                host.BloodDamage = damage;
                host.BloodNew = newDamage;
                if (damage != 0)
                {
                    host.BleedAmount = newDamage > 0 ? newDamage : damage;
                    host.BleedAt = hit.HitPos;
                }
            }
        }

        if (host.DidHit && (hit.Crossed3DWater || hit.CrossedWater))
        {
            if (puff == null)
            {
                puff = Spawn(host, hit.HitPos, 0, 0, 2, puffFlags | PfHitThing | PfTemporary, null);
                killPuff = true;
            }

            host.Splash = true;
            host.SplashPuff = puff;
        }

        host.Fired = true;
        host.FiredFrom = tempos;
        host.FiredPuffPos = puffPos;
        host.FiredPuff = puff;
        host.FiredFlags = flags;
        if (puff != null && puffType.Length > 0 && !killPuff)
        {
            host.Cosmetic = true;
            host.CosmeticAngle = hit.SrcAngle;
        }

        if (killPuff && puff != null)
        {
            puff.Destroyed = true;
            puff = null;
        }

        return puff;
    }

    private static PuffActor Spawn(AttackHost host, Vec3 pos, double hitDir, double particleDir, int upDown, int flags, object? victim)
    {
        var actor = new PuffActor { X = pos.X, Y = pos.Y, Z = pos.Z, HasDecal = host.SpawnHasDecal };
        host.Spawns.Add(new SpawnCall
        {
            Pos = pos,
            HitDir = hitDir,
            ParticleDir = particleDir,
            UpDown = upDown,
            Flags = flags,
            Victim = victim,
            Actor = actor,
        });
        return actor;
    }

    private static double CosDeg(double degrees)
    {
        return Math.Cos(degrees * (Math.PI / 180d));
    }

    private static double SinDeg(double degrees)
    {
        return Math.Sin(degrees * (Math.PI / 180d));
    }
}

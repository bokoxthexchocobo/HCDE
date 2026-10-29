namespace HCDE.Playsim;

/// <summary>
/// Named puff <c>P_LineAttack</c>, <c>P_LineTrace</c>, and <c>P_LinePickActor</c>
/// from <c>p_map.cpp</c> (<b>5051–5237</b>). Chunk <b>1170</b>.
/// </summary>
public static class VmPMapLineTrace1170Slice
{
    public const int TrfAbsPosition = 1;
    public const int TrfAbsOffset = 2;
    public const int TrfThruSpecies = 4;
    public const int TrfThruActors = 8;
    public const int TrfThruBlock = 16;
    public const int TrfThruHitscan = 32;
    public const int TrfNoSky = 64;
    public const int TrfAllActors = 128;
    public const int TrfSolidActors = 256;
    public const int TrfBlockUse = 512;
    public const int TrfBlockSelf = 1024;

    public const int TraceStop = 0;
    public const int TraceSkip = 2;
    public const int TraceCrossingPortal = 5;
    public const int TraceHitActor = 4;
    public const int TraceHitWall = 3;
    public const int TraceHitCeiling = 2;
    public const int TraceNoSky = 0x0001;
    public const int TracePortalRestrict = 0x0008;
    public const int TraceReportPortals = 0x0010;

    public const int MfSolid = 0x00000002;
    public const int MfShootable = 0x00000004;
    public const int MfFloat = 0x00004000;
    public const int MfMissile = 0x00010000;
    public const int MfFriendly = 0x08000000;
    public const int Mf3NoBlockMonst = 0x00100000;
    public const int Mf8BlockAsPlayer = 0x00000004;
    public const int BounceMbf = 1 << 12;
    public const int CompatNoBlockFriends = 1 << 26;

    public const int MlBlocking = 0x00000001;
    public const int MlBlockMonsters = 0x00000002;
    public const int MlBlockPlayers = 0x00004000;
    public const int MlBlockEverything = 0x00008000;
    public const int MlBlockFloaters = 0x00040000;
    public const int MlBlockProjectile = 0x01000000;
    public const int MlBlockUse = 0x02000000;
    public const int MlBlockHitscan = 0x08000000;
    public const int FfUpperTexture = 0x20000;
    public const int FfLowerTexture = 0x40000;
    public const int TierUpper = 1;
    public const int TierLower = 2;
    public const int TierFFloor = 3;

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
        public int Flags;
        public int Flags3;
        public int Flags8;
        public int BounceFlags;
        public int CompatFlags;
        public string Species = "Imp";
        public object? Sector = "sector";
    }

    public sealed class LineData
    {
        public string CallerSpecies = "";
        public bool ThruSpecies;
        public bool ThruActors;
        public int NumPortals;
    }

    public sealed class TraceView
    {
        public int HitType;
        public string Species = "";
        public object? Actor;
        public object? Line;
        public object? Sector;
        public object? Floor3D;
        public int Side;
        public int Tier;
        public int FloorFlags;
        public int[] SideTextures = { 7, 8, 9 };
        public int[] MasterTextures = { 3, 4, 5 };
        public int HitTexture;
        public Vec3 HitPos;
        public Vec3 HitVector;
        public double Distance;
    }

    public sealed class TraceOut
    {
        public object? HitActor;
        public object? HitLine;
        public object? HitSector;
        public object? Hit3DFloor;
        public int SectorPlane = -1;
        public int LineSide;
        public int LinePart;
        public int HitTexture;
        public Vec3 HitLocation;
        public Vec3 HitDir;
        public double Distance;
        public int NumPortals;
        public int HitType;
    }

    public sealed class TraceHost
    {
        public bool TraceHit;
        public TraceView Final = new();
        public List<TraceView> Steps { get; } = new();
        public List<Vec3> SectorLooks { get; } = new();
        public uint ActorMask;
        public uint WallMask;
        public int TraceFlags;
        public Vec3 From;
        public Vec3 Direction;
        public double Distance;
        public bool PickGhosts;
        public string PickSpecies = "unset";

        public Vec3 OffsetZ(double x, double y, double z)
        {
            return new Vec3(100 + x, 200 + y, z);
        }

        public Vec3 PosAtZ(double z)
        {
            return new Vec3(100, 200, z);
        }
    }

    public static int CheckLineTrace(TraceView hit, LineData data)
    {
        if (hit.HitType == TraceCrossingPortal)
        {
            data.NumPortals++;
            return TraceSkip;
        }

        if (hit.HitType != TraceHitActor)
        {
            return TraceStop;
        }

        if (data.ThruActors || (data.ThruSpecies && hit.Species == data.CallerSpecies))
        {
            return TraceSkip;
        }

        return TraceStop;
    }

    public static VmPMapLineAttack1169Slice.PuffActor? LineAttackByName(
        VmPMapLineAttack1169Slice.Shooter shooter,
        double angle,
        double distance,
        double pitch,
        int damage,
        string damageType,
        string puffType,
        int flags,
        VmPMapLineAttack1169Slice.LineTarget? victim,
        VmPMapLineAttack1169Slice.DamageSlot? actualDamage,
        double sz,
        double offsetForward,
        double offsetSide,
        VmPMapLineAttack1169Slice.AttackHost host,
        Func<string, bool> classFound,
        List<string> messages)
    {
        if (!classFound(puffType))
        {
            victim?.Clear();
            messages.Add("Attempt to spawn unknown actor type '" + puffType + "'");
            return null;
        }

        return VmPMapLineAttack1169Slice.LineAttack(
            shooter,
            angle,
            distance,
            pitch,
            damage,
            damageType,
            puffType,
            flags,
            victim,
            actualDamage,
            sz,
            offsetForward,
            offsetSide,
            host);
    }

    public static bool LineTrace(
        Shooter shooter,
        double angle,
        double distance,
        double pitch,
        int flags,
        double sz,
        double offsetForward,
        double offsetSide,
        TraceOut? output,
        TraceHost host)
    {
        var data = new LineData
        {
            CallerSpecies = shooter.Species,
            ThruSpecies = (flags & TrfThruSpecies) != 0,
            ThruActors = (flags & TrfThruActors) != 0,
        };
        var pitchCos = CosDeg(pitch);
        var direction = new Vec3(pitchCos * CosDeg(angle), pitchCos * SinDeg(angle), -SinDeg(pitch));
        var startZ = shooter.Z - shooter.Floorclip + sz;
        Vec3 start;
        if ((flags & TrfAbsPosition) != 0)
        {
            start = new Vec3(offsetForward, offsetSide, sz);
        }
        else if ((flags & TrfAbsOffset) != 0)
        {
            start = host.OffsetZ(offsetForward, offsetSide, startZ);
        }
        else if (offsetForward == 0d && offsetSide == 0d)
        {
            start = host.PosAtZ(startZ);
        }
        else
        {
            var sine = SinDeg(angle);
            var cosine = CosDeg(angle);
            start = host.OffsetZ((offsetForward * cosine) + (offsetSide * sine), (offsetForward * sine) - (offsetSide * cosine), startZ);
        }

        uint actorMask = (flags & TrfAllActors) != 0 ? 0xFFFFFFFFu : MfShootable;
        if ((flags & TrfSolidActors) != 0)
        {
            actorMask |= MfSolid;
        }

        var wallMask = (uint)(MlBlockEverything | MlBlockHitscan);
        if ((flags & TrfBlockUse) != 0)
        {
            wallMask |= MlBlockUse;
        }

        if ((flags & TrfBlockSelf) != 0)
        {
            var projectile = (shooter.Flags & MfMissile) != 0 || (shooter.BounceFlags & BounceMbf) != 0;
            var notBlocked = (shooter.Flags3 & Mf3NoBlockMonst) != 0
                || ((shooter.CompatFlags & CompatNoBlockFriends) != 0 && (shooter.Flags & MfFriendly) != 0);
            if (projectile)
            {
                wallMask |= MlBlockProjectile;
            }

            if (!projectile || (shooter.Flags8 & Mf8BlockAsPlayer) != 0)
            {
                wallMask |= MlBlocking;
            }

            if (!notBlocked)
            {
                wallMask |= MlBlockMonsters;
            }

            if (shooter.Player || (shooter.Flags8 & Mf8BlockAsPlayer) != 0)
            {
                wallMask |= (uint)(MlBlocking | MlBlockPlayers);
            }

            if ((shooter.Flags & MfFloat) != 0)
            {
                wallMask |= MlBlockFloaters;
            }
        }

        if ((flags & TrfThruBlock) != 0)
        {
            wallMask &= ~(uint)MlBlockEverything;
        }

        if ((flags & TrfThruHitscan) != 0)
        {
            wallMask &= ~(uint)MlBlockHitscan;
        }

        var traceFlags = TraceReportPortals;
        if ((flags & TrfNoSky) != 0)
        {
            traceFlags |= TraceNoSky;
        }

        host.SectorLooks.Add(start);
        for (var i = 0; i < host.Steps.Count; i++)
        {
            CheckLineTrace(host.Steps[i], data);
        }

        host.From = start;
        host.Direction = direction;
        host.Distance = distance;
        host.ActorMask = actorMask;
        host.WallMask = wallMask;
        host.TraceFlags = traceFlags;
        if (output != null)
        {
            output.HitActor = null;
            output.HitLine = null;
            output.HitSector = null;
            output.Hit3DFloor = null;
            output.SectorPlane = 0;
            output.LineSide = 0;
            output.LinePart = 0;
            output.HitTexture = 0;
            output.HitLocation = default;
            output.HitDir = default;
            output.Distance = 0;
            output.NumPortals = 0;
            output.HitType = 0;
            var trace = host.Final;
            output.HitActor = trace.Actor;
            output.HitLine = trace.Line;
            output.HitSector = trace.Sector;
            output.Hit3DFloor = trace.Floor3D;
            output.SectorPlane = trace.HitType == TraceHitCeiling ? 1 : 0;
            output.HitType = trace.HitType;
            output.HitLocation = trace.HitPos;
            output.HitDir = trace.HitVector;
            output.Distance = trace.Distance;
            output.NumPortals = data.NumPortals - 2;
            if (trace.HitType == TraceHitWall)
            {
                output.LineSide = trace.Side;
                if (trace.Tier == 0)
                {
                    output.LinePart = 1;
                    output.HitTexture = trace.SideTextures[1];
                }
                else if (trace.Tier == TierUpper)
                {
                    output.LinePart = 0;
                    output.HitTexture = trace.SideTextures[0];
                }
                else if (trace.Tier == TierLower)
                {
                    output.LinePart = 2;
                    output.HitTexture = trace.SideTextures[2];
                }
                else if (trace.Tier == TierFFloor)
                {
                    output.LinePart = 1;
                    var texturePart = (trace.FloorFlags & FfUpperTexture) != 0
                        ? 0
                        : (trace.FloorFlags & FfLowerTexture) != 0 ? 2 : 1;
                    output.HitTexture = trace.MasterTextures[texturePart];
                }
            }
            else
            {
                output.HitTexture = trace.HitTexture;
            }
        }

        return host.TraceHit;
    }

    public static object? LinePickActor(
        Shooter shooter,
        double angle,
        double distance,
        double pitch,
        uint actorMask,
        uint wallMask,
        TraceHost host)
    {
        var pitchCos = CosDeg(pitch);
        var direction = new Vec3(pitchCos * CosDeg(angle), pitchCos * SinDeg(angle), -SinDeg(pitch));
        var shootZ = shooter.Center - shooter.Floorclip + shooter.AttackOffset;
        host.From = host.PosAtZ(shootZ);
        host.Direction = direction;
        host.Distance = distance;
        host.ActorMask = actorMask;
        host.WallMask = wallMask;
        host.TraceFlags = TraceNoSky | TracePortalRestrict;
        host.PickGhosts = true;
        host.PickSpecies = "None";
        if (host.TraceHit && host.Final.HitType == TraceHitActor)
        {
            return host.Final.Actor;
        }

        return null;
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

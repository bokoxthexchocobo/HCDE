namespace HCDE.Playsim;

/// <summary>ACS thing spawn/move and velocity intercept from <c>p_things.cpp</c> (41–248).</summary>
public static class VmPThingsSpawnMoveIntercept
{
    public readonly record struct Vec3(double X, double Y, double Z)
    {
        public double Length() => Math.Sqrt(X * X + Y * Y + Z * Z);
        public bool IsZero() => X == 0 && Y == 0 && Z == 0;
        public Vec3 WithZ(double z) => this with { Z = z };
        public Vec3 Resized(double len)
        {
            var vlen = Length();
            if (vlen == 0) return this;
            var scale = len / vlen;
            return new(X * scale, Y * scale, Z * scale);
        }
        public Vec3 Normalized()
        {
            var len = Length();
            if (len == 0) return this;
            var inv = 1 / len;
            return new(X * inv, Y * inv, Z * inv);
        }
        public static double Dot(Vec3 a, Vec3 b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        public static Vec3 Cross(Vec3 a, Vec3 b) =>
            new(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);
        public static Vec3 operator *(Vec3 v, double s) => new(v.X * s, v.Y * s, v.Z * s);
        public static Vec3 operator +(Vec3 a, Vec3 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    }

    public const int Mf3Ismonster = 1;
    public const int Mf2Passmobj = 1 << 12;
    public const int MfSpecial = 1 << 2;
    public const int MfDropped = 1 << 17;
    public const int MfNogravity = 1 << 10;
    public const int DfNoMonsters = 1;
    public const int Level2Nomonsters = 1;
    public const int RfNointerpolateview = 1 << 3;
    public const double AngleSentinelDegrees = 1_000_000;

    public sealed class MoveThingActor
    {
        public double PosX { get; set; }
        public double PosY { get; set; }
        public double PosZ { get; set; }
        public int RenderFlags { get; set; }
    }

    public sealed class MoveThingContext
    {
        public required MoveThingActor Source { get; init; }
        public bool Fog { get; init; }
        public Action<double, double, double, bool, bool>? SpawnTeleportFog { get; init; }
        public Func<bool>? TestMobjLocation { get; init; }
        public Action? ClearInterpolation { get; init; }
    }

    public static bool P_MoveThing(MoveThingContext ctx, double posX, double posY, double posZ)
    {
        var source = ctx.Source;
        var oldX = source.PosX;
        var oldY = source.PosY;
        var oldZ = source.PosZ;
        source.PosX = posX;
        source.PosY = posY;
        source.PosZ = posZ;
        if (ctx.TestMobjLocation?.Invoke() == true)
        {
            if (ctx.Fog)
            {
                ctx.SpawnTeleportFog?.Invoke(source.PosX, source.PosY, source.PosZ, false, true);
                ctx.SpawnTeleportFog?.Invoke(oldX, oldY, oldZ, true, true);
            }
            ctx.ClearInterpolation?.Invoke();
            source.RenderFlags |= RfNointerpolateview;
            return true;
        }
        source.PosX = oldX;
        source.PosY = oldY;
        source.PosZ = oldZ;
        return false;
    }

    public sealed class EvThingMoveContext
    {
        public int Tid { get; init; }
        public object? Source { get; set; }
        public int MapspotTid { get; init; }
        public required MoveThingContext Move { get; init; }
        public Func<int, object?>? TidIteratorFirst { get; init; }
        public Func<object?, (double X, double Y, double Z)>? ActorPos { get; init; }
    }

    public static bool EvThingMove(EvThingMoveContext ctx)
    {
        var source = ctx.Source;
        if (ctx.Tid != 0)
            source = ctx.TidIteratorFirst?.Invoke(ctx.Tid);
        var target = ctx.TidIteratorFirst?.Invoke(ctx.MapspotTid);
        if (source == null || target == null)
            return false;
        var pos = ctx.ActorPos?.Invoke(target) ?? (0, 0, 0);
        return P_MoveThing(ctx.Move, pos.X, pos.Y, pos.Z);
    }

    public sealed class SpawnSpotActor
    {
        public double PosX { get; set; }
        public double PosY { get; set; }
        public double PosZ { get; set; }
        public double YawDegrees { get; set; }
        public int Flags { get; set; }
        public int Flags2 { get; set; }
    }

    public sealed class SpawnedMobj
    {
        public int Flags { get; set; }
        public int Flags2 { get; set; }
        public double YawDegrees { get; set; }
        public int NewTid { get; set; }
        public bool Destroyed { get; set; }
    }

    public sealed class EvThingSpawnContext
    {
        public int Tid { get; init; }
        public object? Source { get; init; }
        public int Type { get; init; }
        public double AngleDegrees { get; init; }
        public bool Fog { get; init; }
        public int NewTid { get; init; }
        public int DmFlags { get; init; }
        public int LevelFlags2 { get; init; }
        public Func<int, object?>? GetSpawnableKind { get; init; }
        public Func<object?, object?>? GetReplacement { get; init; }
        public Func<object?, int>? GetDefaultFlags3 { get; init; }
        public Func<int, Func<object?>>? CreateSpotIterator { get; init; }
        public Func<SpawnSpotActor, object?, SpawnedMobj?>? Spawn { get; init; }
        public Func<SpawnedMobj, bool>? TestMobjLocation { get; init; }
        public Action<SpawnedMobj, double, double, double, bool, bool>? SpawnTeleportFog { get; init; }
        public Action<SpawnedMobj>? ClearCounters { get; init; }
        public Action<SpawnedMobj>? DestroyMobj { get; init; }
        public Func<object?, SpawnSpotActor>? AsSpot { get; init; }
    }

    public static bool EvThingSpawn(EvThingSpawnContext ctx)
    {
        var rtn = 0;
        var kind = ctx.GetSpawnableKind?.Invoke(ctx.Type);
        if (kind == null)
            return false;
        kind = ctx.GetReplacement?.Invoke(kind) ?? kind;
        var defFlags3 = ctx.GetDefaultFlags3?.Invoke(kind) ?? 0;
        if ((defFlags3 & Mf3Ismonster) != 0
            && ((ctx.DmFlags & DfNoMonsters) != 0 || (ctx.LevelFlags2 & Level2Nomonsters) != 0))
            return false;

        var nextSpot = ctx.CreateSpotIterator?.Invoke(ctx.Tid);
        object? spotObj = ctx.Tid == 0 ? ctx.Source : nextSpot?.Invoke();
        while (spotObj != null)
        {
            var spot = ctx.AsSpot?.Invoke(spotObj) ?? new SpawnSpotActor();
            var mobj = ctx.Spawn?.Invoke(spot, kind);
            if (mobj != null)
            {
                var oldFlags2 = mobj.Flags2;
                mobj.Flags2 |= Mf2Passmobj;
                if (ctx.TestMobjLocation?.Invoke(mobj) == true)
                {
                    rtn++;
                    mobj.YawDegrees = ctx.AngleDegrees != AngleSentinelDegrees ? ctx.AngleDegrees : spot.YawDegrees;
                    if (ctx.Fog)
                        ctx.SpawnTeleportFog?.Invoke(mobj, spot.PosX, spot.PosY, spot.PosZ, false, true);
                    if ((mobj.Flags & MfSpecial) != 0)
                        mobj.Flags |= MfDropped;
                    mobj.NewTid = ctx.NewTid;
                    mobj.Flags2 = oldFlags2;
                }
                else
                {
                    ctx.ClearCounters?.Invoke(mobj);
                    ctx.DestroyMobj?.Invoke(mobj);
                }
            }
            spotObj = ctx.Tid == 0 ? null : nextSpot?.Invoke();
        }
        return rtn != 0;
    }

    public sealed class InterceptActor
    {
        public double PosX { get; set; }
        public double PosY { get; set; }
        public double PosZ { get; set; }
        public double VelX { get; set; }
        public double VelY { get; set; }
        public double VelZ { get; set; }
        public double YawDegrees { get; set; }
        public double PitchDegrees { get; set; }
        public double Speed { get; set; }
        public int Flags { get; set; }
    }

    public sealed class InterceptTarget
    {
        public double PosX { get; set; }
        public double PosY { get; set; }
        public double PosZ { get; set; }
        public double Height { get; set; }
        public double VelX { get; set; }
        public double VelY { get; set; }
        public double VelZ { get; set; }
        public int Flags { get; set; }
        public int WaterLevel { get; set; }
    }

    public static void InterceptDefaultAim(InterceptActor mobj, InterceptTarget targ, Vec3 aim, double speed)
    {
        if (mobj == null || targ == null) return;
        mobj.YawDegrees = Math.Atan2(targ.PosY - mobj.PosY, targ.PosX - mobj.PosX) * 180 / Math.PI;
        var vel = aim.Resized(speed);
        mobj.VelX = vel.X;
        mobj.VelY = vel.Y;
        mobj.VelZ = vel.Z;
    }

    public static Vec3 Vec3To(InterceptActor from, InterceptTarget to) =>
        new(to.PosX - from.PosX, to.PosY - from.PosY, to.PosZ - from.PosZ);

    public static void AngleFromVel(InterceptActor mobj)
    {
        mobj.YawDegrees = Math.Atan2(mobj.VelY, mobj.VelX) * 180 / Math.PI;
    }

    public static double VecToAngle(double dist, double dz) =>
        dist == 0 ? (dz >= 0 ? double.PositiveInfinity : double.NegativeInfinity) : Math.Atan2(dz, dist);

    public static void VelIntercept(
        InterceptTarget targ,
        InterceptActor mobj,
        double speed,
        bool aimpitch = false,
        bool oldvel = false,
        bool resetvel = false,
        bool leadtarget = true,
        Func<int>? leadTargetRandom2 = null)
    {
        if (targ == null || mobj == null) return;

        var prevel = new Vec3(mobj.VelX, mobj.VelY, mobj.VelZ);
        var aim = Vec3To(mobj, targ);
        aim = aim.WithZ(aim.Z + targ.Height / 2);

        if (leadtarget && speed > 0 && !(targ.VelX == 0 && targ.VelY == 0 && targ.VelZ == 0))
        {
            var tvel = new Vec3(targ.VelX, targ.VelY, targ.VelZ);
            if ((targ.Flags & MfNogravity) == 0 && targ.WaterLevel < 3)
            {
                tvel = tvel.WithZ(0);
                if (targ.VelX == 0 && targ.VelY == 0)
                {
                    InterceptDefaultAim(mobj, targ, aim, speed);
                    if (resetvel)
                    {
                        mobj.VelX = prevel.X;
                        mobj.VelY = prevel.Y;
                        mobj.VelZ = prevel.Z;
                    }
                    return;
                }
            }
            var dist = aim.Length();
            var targspeed = tvel.Length();
            var ydotx = -Vec3.Dot(aim, tvel);
            var a = Math.Acos(Clamp(ydotx / targspeed / dist, -1.0, 1.0));
            var multiplier = (leadTargetRandom2?.Invoke() ?? 0) * 0.1 / 255 + 1.1;
            var sinb = -Clamp(targspeed * multiplier * Math.Sin(a) / speed, -1.0, 1.0);
            var rv = Vec3.Cross(tvel, aim).Normalized();
            var cosB = Math.Cos(Math.Asin(sinb));
            var rm = RotationMatrixFromAxis(rv, cosB, sinb);
            var aimvec = MultiplyMatrixVector(rm, aim);
            var scaled = aimvec * (speed / dist);
            mobj.VelX = scaled.X;
            mobj.VelY = scaled.Y;
            mobj.VelZ = scaled.Z;
            AngleFromVel(mobj);
            if (oldvel)
            {
                mobj.VelX = prevel.X;
                mobj.VelY = prevel.Y;
                mobj.VelZ = prevel.Z;
            }
            if (aimpitch)
            {
                var horiz = Math.Sqrt(mobj.VelX * mobj.VelX + mobj.VelY * mobj.VelY);
                mobj.PitchDegrees = -VecToAngle(horiz, mobj.VelZ) * 180 / Math.PI;
            }
        }
        else
        {
            InterceptDefaultAim(mobj, targ, aim, speed);
        }
        if (resetvel)
        {
            mobj.VelX = prevel.X;
            mobj.VelY = prevel.Y;
            mobj.VelZ = prevel.Z;
        }
    }

    public sealed class VelInterceptVmArgs
    {
        public required InterceptTarget Target { get; init; }
        public required InterceptActor Self { get; init; }
        public double Speed { get; set; }
        public bool AimPitch { get; init; }
        public bool OldVel { get; init; }
        public bool ResetVel { get; init; }
        public Func<int>? LeadTargetRandom2 { get; init; }
    }

    public static int VelInterceptAction(VelInterceptVmArgs args)
    {
        var speed = args.Speed;
        if (speed < 0)
            speed = args.Self.Speed;
        VelIntercept(args.Target, args.Self, speed, args.AimPitch, args.OldVel, args.ResetVel,
            leadtarget: true, leadTargetRandom2: args.LeadTargetRandom2);
        return 0;
    }

    static double Clamp(double v, double lo, double hi) => v < lo ? lo : v > hi ? hi : v;

    static (Vec3 R0, Vec3 R1, Vec3 R2) RotationMatrixFromAxis(Vec3 axis, double c, double s)
    {
        var t = 1 - c;
        var sx = s * axis.X;
        var sy = s * axis.Y;
        var sz = s * axis.Z;
        var tx = t * axis.X;
        var txx = tx * axis.X;
        var u = tx * axis.Y - sz;
        var v = tx * axis.Z + sy;
        var r0 = new Vec3(txx + c, u, v);
        var ty = t * axis.Y;
        var tyy = ty * axis.Y;
        var r1u = ty * axis.Z - sx;
        var r1 = new Vec3(u + sz, tyy + c, r1u);
        v = tx * axis.Z - sy;
        var r2 = new Vec3(v - sy, r1u + sx, t - txx - tyy + c);
        return (r0, r1, r2);
    }

    static Vec3 MultiplyMatrixVector((Vec3 R0, Vec3 R1, Vec3 R2) m, Vec3 v) =>
        new(
            Vec3.Dot(m.R0, v),
            Vec3.Dot(m.R1, v),
            Vec3.Dot(m.R2, v));
}

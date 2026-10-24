namespace HCDE.Playsim;

/// <summary>Seeker/tracer actions and <c>CheckIfSeen</c> from <c>p_actionfunctions.cpp</c> (742–887, 1854–1878).</summary>
public static class VmPActionSeekerTracer
{
    public const int SmfLook = 1;
    public const int SmfPrecise = 2;
    public const int SmfCurspeed = 4;
    public const int Mf3Floorhugger = 1;
    public const int Mf3Ceilinghugger = 1 << 1;
    public const int MaxPlayers = 8;

    public sealed class SeekerActor
    {
        public object? Target { get; set; }
        public object? Tracer { get; set; }
        public bool TargetIsPlayer { get; set; }
        public bool TracerIsPlayer { get; set; }
    }

    public sealed class SeekerContext
    {
        public Func<int, int, bool>? RandomLessThanChance { get; init; }
        public Func<object?, int, bool, object?>? RoughMonsterSearch { get; init; }
        public Func<SeekerActor, int, int, bool, bool, bool>? SeekerMissileSteer { get; init; }
    }

    public static void SeekerMissile(
        SeekerContext ctx,
        SeekerActor self,
        int ang1,
        int ang2,
        int flags,
        int chance,
        int distance)
    {
        if ((flags & SmfLook) == 0)
        {
            if (self.Target != null && self.TargetIsPlayer
                && (self.Tracer == null || !self.TracerIsPlayer))
            {
                self.Tracer = self.Target;
            }
        }

        if ((flags & SmfLook) != 0 && self.Tracer == null
            && ctx.RandomLessThanChance?.Invoke(chance, 256) == true)
        {
            self.Tracer = ctx.RoughMonsterSearch?.Invoke(self, distance, true);
        }

        var threshold = Math.Max(ang1, 0);
        var turnMax = Math.Max(ang2, 0);
        var ok = ctx.SeekerMissileSteer?.Invoke(self, threshold, turnMax,
            (flags & SmfPrecise) != 0, (flags & SmfCurspeed) != 0) ?? false;
        if (!ok && (flags & SmfLook) != 0)
            self.Tracer = null;
    }

    public sealed class TracerActor
    {
        public object? Tracer { get; set; }
        public int TracerHealth { get; set; }
        public double Speed { get; set; }
        public bool CanSeek { get; set; }
        public double Yaw { get; set; }
        public double VelZ { get; set; }
        public double Z { get; set; }
        public double Height { get; set; }
        public int Flags3 { get; set; }
        public double TracerZ { get; set; }
        public double TracerHeight { get; set; }
    }

    public class TracerContext
    {
        public Func<TracerActor, double>? AngleTo { get; init; }
        public Func<double, double, double>? DeltaAngle { get; init; }
        public Action<TracerActor>? VelFromAngle { get; init; }
        public Func<TracerActor, TracerActor, double, double>? DistanceBySpeed { get; init; }
    }

    public static void DoTracer2(TracerContext ctx, TracerActor self, double traceAng)
    {
        if (self.Tracer == null || self.TracerHealth <= 0 || self.Speed == 0 || !self.CanSeek)
            return;

        var exact = ctx.AngleTo?.Invoke(self) ?? self.Yaw;
        var diff = ctx.DeltaAngle?.Invoke(self.Yaw, exact) ?? 0;

        if (diff < 0)
        {
            self.Yaw -= traceAng;
            if ((ctx.DeltaAngle?.Invoke(self.Yaw, exact) ?? 0) > 0)
                self.Yaw = exact;
        }
        else if (diff > 0)
        {
            self.Yaw += traceAng;
            if ((ctx.DeltaAngle?.Invoke(self.Yaw, exact) ?? 0) < 0)
                self.Yaw = exact;
        }

        ctx.VelFromAngle?.Invoke(self);

        if ((self.Flags3 & (Mf3Floorhugger | Mf3Ceilinghugger)) != 0)
            return;

        var dist = ctx.DistanceBySpeed?.Invoke(self, self, self.Speed) ?? 1.0;
        var slope = self.TracerHeight >= 56
            ? (self.TracerZ + 40 - self.Z) / dist
            : (self.TracerZ + self.Height * (2.0 / 3) - self.Z) / dist;

        if (slope < self.VelZ)
            self.VelZ -= 1.0 / 8;
        else
            self.VelZ += 1.0 / 8;
    }

    public sealed class TracerSmokeContext : TracerContext
    {
        public int MapTime { get; init; }
        public Action<TracerActor>? SpawnPuff { get; init; }
        public Action<TracerActor, int>? SpawnSmoke { get; init; }
        public Func<int, int>? TracerRandomMod4 { get; init; }
    }

    public static void TracerAction(TracerSmokeContext ctx, TracerActor self)
    {
        if ((ctx.MapTime & 3) != 0)
            return;
        ctx.SpawnPuff?.Invoke(self);
        var mod = ctx.TracerRandomMod4?.Invoke(4) ?? 0;
        ctx.SpawnSmoke?.Invoke(self, mod);
        DoTracer2(ctx, self, 16.875);
    }

    public sealed class SightContext
    {
        public Func<int, bool>? PlayerInGame { get; init; }
        public Func<int, object?>? PlayerMo { get; init; }
        public Func<object?, object?, bool>? CheckSight { get; init; }
        public Func<int, object?>? Camera { get; init; }
        public Func<object?, bool>? CameraIsNonPlayer { get; init; }
    }

    public static bool CheckIfSeen(SightContext ctx, object self)
    {
        for (var i = 0; i < MaxPlayers; i++)
        {
            if (ctx.PlayerInGame?.Invoke(i) != true)
                continue;
            var mo = ctx.PlayerMo?.Invoke(i);
            if (mo != null && ctx.CheckSight?.Invoke(mo, self) == true)
                return false;
            var camera = ctx.Camera?.Invoke(i);
            if (camera != null && ctx.CameraIsNonPlayer?.Invoke(camera) == true
                && ctx.CheckSight?.Invoke(camera, self) == true)
            {
                return false;
            }
        }

        return true;
    }
}

namespace HCDE.Playsim;

/// <summary><c>DEarthquake</c> damage/intensity helpers from <c>mapthinkers/a_quake.cpp</c> (174–463).</summary>
public static class VmAQuake
{
    public const int QfShakeonly = 1 << 0;
    public const int QfScaledown = 1 << 1;
    public const int QfScaleup = 1 << 2;
    public const int QfMax = 1 << 3;
    public const int QfFullintensity = 1 << 4;
    public const double Ticrate = 35;

    public sealed class QuakeThinker
    {
        public object? Spot { get; set; }
        public double DamageRadius { get; set; }
        public double TremorRadius { get; set; }
        public double Falloff { get; set; }
        public int Flags { get; set; }
        public int Countdown { get; set; }
        public int CountdownStart { get; set; }
        public int Highpoint { get; set; }
        public int MiniCount { get; set; }
        public double IntensityX { get; set; }
        public double IntensityY { get; set; }
        public double ThrustMultiplier { get; set; }
        public double DamageMultiplier { get; set; } = 1;
        public int Damage { get; set; }
        public double RollIntensity { get; set; }
        public Func<object?, object?, double> Distance2D { get; init; } = (_, _) => 0;
    }

    public sealed class QuakeVictim
    {
        public double Z { get; set; }
        public double FloorZ { get; set; }
        public int Flags7DontThrust { get; set; }
        public double VelX { get; set; }
        public double VelY { get; set; }
        public double Yaw { get; set; }
        public Action<int> ApplyDamage { get; init; } = _ => { };
    }

    public sealed class StartQuakeContext
    {
        public Action<object> SpawnQuakeAt { get; init; } = _ => { };
        public Func<int, IEnumerable<object>> ActorsByTid { get; init; } = _ => Array.Empty<object>();
    }

    public static void DoQuakeDamage(QuakeThinker quake, QuakeVictim victim, bool falloff, Func<int> randomQuake, Func<int, int> hitDice)
    {
        if (quake.Spot == null) return;
        var dist = quake.Distance2D(quake.Spot, victim);
        var thrustFalloff = falloff ? GetFalloff(quake, dist, quake.DamageRadius) : 1.0;
        if (dist < quake.DamageRadius && victim.Z <= victim.FloorZ)
        {
            if ((quake.Flags & QfShakeonly) == 0 && randomQuake() < 50)
            {
                int damage;
                if (quake.Damage < 1)
                    damage = falloff
                        ? (int)(hitDice(1) * GetFalloff(quake, dist, quake.DamageRadius) * quake.DamageMultiplier)
                        : (int)(hitDice(1) * quake.DamageMultiplier);
                else
                    damage = falloff
                        ? (int)(quake.Damage * GetFalloff(quake, dist, quake.DamageRadius) * quake.DamageMultiplier)
                        : (int)(quake.Damage * quake.DamageMultiplier);
                if (damage < 1) damage = 1;
                victim.ApplyDamage(damage);
            }

            if (victim.Flags7DontThrust == 0 && quake.ThrustMultiplier > 0)
            {
                var angle = victim.Yaw + randomQuake();
                victim.VelX += quake.IntensityX * Math.Cos(angle) * quake.ThrustMultiplier * thrustFalloff;
                victim.VelY += quake.IntensityY * Math.Sin(angle) * quake.ThrustMultiplier * thrustFalloff;
            }
        }
    }

    public static double GetModWave(QuakeThinker quake, double ticFrac, double waveMultiplier) =>
        Math.Sin(waveMultiplier * (quake.Countdown - ticFrac) * (Math.PI * 2 / Ticrate));

    public static double GetModIntensity(QuakeThinker quake, double intensity, bool fake)
    {
        intensity += intensity;
        if ((quake.Flags & (QfScaledown | QfScaleup)) == 0) return intensity;

        var check = quake.Highpoint > 0 && quake.Highpoint < quake.CountdownStart;
        var divider = check ? quake.Highpoint : quake.CountdownStart;
        int scalar;
        if ((quake.Flags & (QfScaledown | QfScaleup)) == (QfScaledown | QfScaleup))
        {
            if (check)
            {
                scalar = quake.MiniCount > 0
                    ? ((quake.Flags & QfMax) != 0 ? quake.MiniCount : quake.Highpoint - quake.MiniCount)
                    : ((quake.Flags & QfMax) != 0 ? quake.CountdownStart - quake.Highpoint - quake.Countdown : quake.Countdown);
            }
            else
            {
                divider = quake.CountdownStart;
                scalar = (quake.Flags & QfMax) != 0
                    ? Math.Max(quake.Countdown, quake.CountdownStart - quake.Countdown)
                    : Math.Min(quake.Countdown, quake.CountdownStart - quake.Countdown);
            }

            if (scalar > divider) scalar = divider;
            if (!fake && (quake.Flags & QfFullintensity) != 0) scalar *= 2;
        }
        else if ((quake.Flags & QfScaledown) != 0)
            scalar = quake.Countdown;
        else
        {
            scalar = quake.CountdownStart - quake.Countdown;
            if (quake.Highpoint > 0)
                scalar = (quake.Highpoint - quake.MiniCount) < divider ? quake.Highpoint - quake.MiniCount : divider;
            if (scalar > divider) scalar = divider;
        }

        return divider > 0 ? intensity * scalar / divider : intensity;
    }

    public static double GetFalloff(QuakeThinker quake, double dist, double radius)
    {
        if (dist < quake.Falloff || quake.Falloff >= radius || quake.Falloff <= 0 || radius - quake.Falloff <= 0)
            return 1.0;
        if (dist > quake.Falloff && dist < radius)
        {
            var tremorSize = radius - quake.Falloff;
            return tremorSize > 0 ? 1.0 - ((dist - quake.Falloff) / tremorSize) : 1.0;
        }

        return 1.0;
    }

    public static bool StartQuakeXyz(
        StartQuakeContext ctx,
        object? activator,
        int tid,
        ref double intensityX,
        ref double intensityY,
        ref double intensityZ,
        int duration,
        double damrad,
        double tremrad,
        object quakesfx,
        int flags,
        double waveSpeedX,
        double waveSpeedY,
        double waveSpeedZ,
        double falloff,
        int highpoint,
        double rollIntensity,
        double rollWave,
        double damageMultiplier,
        double thrustMultiplier,
        int damage)
    {
        intensityX = Math.Clamp(intensityX, 0, 9);
        intensityY = Math.Clamp(intensityY, 0, 9);
        intensityZ = Math.Clamp(intensityZ, 0, 9);
        if (tid == 0)
        {
            if (activator == null) return false;
            ctx.SpawnQuakeAt(activator);
            return true;
        }

        var res = false;
        foreach (var center in ctx.ActorsByTid(tid))
        {
            res = true;
            ctx.SpawnQuakeAt(center);
        }

        return res;
    }

    public static bool StartQuake(StartQuakeContext ctx, object? activator, int tid, double intensity, int duration, double damrad, double tremrad, object quakesfx)
    {
        var ix = intensity;
        var iy = intensity;
        var iz = 0.0;
        return StartQuakeXyz(ctx, activator, tid, ref ix, ref iy, ref iz, duration, damrad, tremrad, quakesfx, 0, 0, 0, 0, 0, 0, 0, 0, 1.0, 0.5, 0);
    }
}

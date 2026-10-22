namespace HCDE.Playsim;

/// <summary><c>DEarthquake</c> from <c>mapthinkers/a_quake.cpp</c> (47–417, 174–463).</summary>
public static class VmAQuake
{
    public const int QfShakeonly = 1 << 0;
    public const int QfScaledown = 1 << 1;
    public const int QfScaleup = 1 << 2;
    public const int QfMax = 1 << 3;
    public const int QfFullintensity = 1 << 4;
    public const int QfAffectactors = 1 << 5;
    public const int QfDamagefalloff = 1 << 6;
    public const int QfGroundonly = 1 << 7;
    public const int Qf3d = 1 << 8;
    public const int QfWave = 1 << 9;
    public const int QfRelative = 1 << 10;
    public const int CfNoclip = 1 << 0;
    public const int ChanBody = 0;
    public const double Ticrate = 35;

    public sealed class QuakeJiggers
    {
        public double RollIntensity { get; set; }
        public double RollWave { get; set; }
        public double IntensityX { get; set; }
        public double IntensityY { get; set; }
        public double IntensityZ { get; set; }
        public double RelIntensityX { get; set; }
        public double RelIntensityY { get; set; }
        public double RelIntensityZ { get; set; }
        public double OffsetX { get; set; }
        public double OffsetY { get; set; }
        public double OffsetZ { get; set; }
        public double RelOffsetX { get; set; }
        public double RelOffsetY { get; set; }
        public double RelOffsetZ { get; set; }
    }

    public sealed class QuakeThinker
    {
        public object? Spot { get; set; }
        public object QuakeSfx { get; set; } = new();
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
        public double IntensityZ { get; set; }
        public double WaveSpeedX { get; set; }
        public double WaveSpeedY { get; set; }
        public double WaveSpeedZ { get; set; }
        public double ThrustMultiplier { get; set; }
        public double DamageMultiplier { get; set; } = 1;
        public int Damage { get; set; }
        public double RollIntensity { get; set; }
        public double RollWave { get; set; }
        public Func<object?, object?, double> Distance2D { get; init; } = (_, _) => 0;
        public Func<object?, object?, double> Distance3D { get; init; } = (_, _) => 0;
        public Action DestroySelf { get; init; } = () => { };
    }

    public sealed class QuakeTickContext
    {
        public Func<object, object, bool> IsActorPlayingSound { get; init; } = (_, _) => false;
        public Action<object, object> PlayLoopSound { get; init; } = (_, _) => { };
        public Action<object, int> StopSound { get; init; } = (_, _) => { };
        public Func<int, bool> PlayerInGame { get; init; } = _ => false;
        public Func<int, int> PlayerCheats { get; init; } = _ => 0;
        public Func<int, QuakeVictim?> PlayerMobj { get; init; } = _ => null;
        public Func<object, double, IEnumerable<QuakeVictim>> ThingsInRadius { get; init; } = (_, _) => Array.Empty<QuakeVictim>();
        public Func<int> RandomQuake { get; init; } = () => 0;
        public Func<int, int> HitDice { get; init; } = _ => 1;
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

    public static void Construct(
        QuakeThinker quake,
        object? center,
        double intensityX,
        double intensityY,
        double intensityZ,
        int duration,
        double damrad,
        double tremrad,
        object quakesound,
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
        quake.QuakeSfx = quakesound;
        quake.Spot = center;
        quake.DamageRadius = damrad;
        quake.TremorRadius = tremrad;
        quake.IntensityX = intensityX;
        quake.IntensityY = intensityY;
        quake.IntensityZ = intensityZ;
        quake.CountdownStart = duration;
        quake.Countdown = duration;
        quake.Flags = flags;
        quake.WaveSpeedX = waveSpeedX;
        quake.WaveSpeedY = waveSpeedY;
        quake.WaveSpeedZ = waveSpeedZ;
        quake.Falloff = falloff;
        quake.Highpoint = highpoint;
        quake.MiniCount = highpoint;
        quake.RollIntensity = rollIntensity;
        quake.RollWave = rollWave;
        quake.DamageMultiplier = damageMultiplier;
        quake.ThrustMultiplier = thrustMultiplier;
        quake.Damage = damage;
    }

    public static void Tick(QuakeThinker quake, QuakeTickContext ctx)
    {
        if (quake.Spot == null)
        {
            quake.DestroySelf();
            return;
        }

        if (!ctx.IsActorPlayingSound(quake.Spot, quake.QuakeSfx))
            ctx.PlayLoopSound(quake.Spot, quake.QuakeSfx);

        if (quake.DamageRadius > 0)
        {
            var falloff = (quake.Flags & QfDamagefalloff) != 0;
            if ((quake.Flags & QfAffectactors) != 0)
            {
                foreach (var victim in ctx.ThingsInRadius(quake.Spot!, quake.DamageRadius))
                {
                    if (victim == null) continue;
                    DoQuakeDamage(quake, victim, falloff, ctx.RandomQuake, ctx.HitDice);
                }
            }
            else
            {
                for (var i = 0; i < 8; i++)
                {
                    if (!ctx.PlayerInGame(i) || (ctx.PlayerCheats(i) & CfNoclip) != 0) continue;
                    var victim = ctx.PlayerMobj(i);
                    if (victim != null)
                        DoQuakeDamage(quake, victim, falloff, ctx.RandomQuake, ctx.HitDice);
                }
            }
        }

        if (quake.MiniCount > 0) quake.MiniCount--;
        if (--quake.Countdown == 0)
        {
            if (ctx.IsActorPlayingSound(quake.Spot, quake.QuakeSfx))
                ctx.StopSound(quake.Spot, ChanBody);
            quake.DestroySelf();
        }
    }

    public static int StaticGetQuakeIntensities(
        double ticFrac,
        QuakeVictim victim,
        QuakeJiggers jiggers,
        IReadOnlyList<QuakeThinker> quakes,
        Func<QuakeVictim, bool> playerHasNoclip)
    {
        if (playerHasNoclip(victim)) return 0;
        var count = 0;
        foreach (var quake in quakes)
        {
            if (quake.Spot == null) continue;
            if ((quake.Flags & QfGroundonly) != 0 && victim.Z > victim.FloorZ) continue;
            var dist = (quake.Flags & Qf3d) != 0
                ? quake.Distance3D(quake.Spot, victim)
                : quake.Distance2D(quake.Spot, victim);
            if (dist >= quake.TremorRadius) continue;
            count++;
            var falloff = GetFalloff(quake, dist, quake.TremorRadius);
            var r = GetModIntensity(quake, quake.RollIntensity, false);
            var strength = GetModIntensity(quake, 1.0, true);
            var ix = GetModIntensity(quake, quake.IntensityX, false);
            var iy = GetModIntensity(quake, quake.IntensityY, false);
            var iz = GetModIntensity(quake, quake.IntensityZ, false);
            if ((quake.Flags & QfWave) == 0)
            {
                jiggers.RollIntensity = Math.Max(r, jiggers.RollIntensity) * falloff;
                ix *= falloff;
                iy *= falloff;
                iz *= falloff;
                if ((quake.Flags & QfRelative) != 0)
                {
                    jiggers.RelIntensityX = Math.Max(ix, jiggers.RelIntensityX);
                    jiggers.RelIntensityY = Math.Max(iy, jiggers.RelIntensityY);
                    jiggers.RelIntensityZ = Math.Max(iz, jiggers.RelIntensityZ);
                }
                else
                {
                    jiggers.IntensityX = Math.Max(ix, jiggers.IntensityX);
                    jiggers.IntensityY = Math.Max(iy, jiggers.IntensityY);
                    jiggers.IntensityZ = Math.Max(iz, jiggers.IntensityZ);
                }
            }
            else
            {
                jiggers.RollWave = r * GetModWave(quake, ticFrac, quake.RollWave) * falloff * strength;
                ix *= GetModWave(quake, ticFrac, quake.WaveSpeedX) * strength * falloff;
                iy *= GetModWave(quake, ticFrac, quake.WaveSpeedY) * strength * falloff;
                iz *= GetModWave(quake, ticFrac, quake.WaveSpeedZ) * strength * falloff;
                if ((quake.Flags & QfRelative) != 0)
                {
                    jiggers.RelOffsetX += ix;
                    jiggers.RelOffsetY += iy;
                    jiggers.RelOffsetZ += iz;
                }
                else
                {
                    jiggers.OffsetX += ix;
                    jiggers.OffsetY += iy;
                    jiggers.OffsetZ += iz;
                }
            }
        }

        return count;
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

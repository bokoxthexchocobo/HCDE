namespace HCDE.Playsim;

/// <summary><c>DEarthquake</c> construct/tick/static intensity from <c>mapthinkers/a_quake.cpp</c> (47–417).</summary>
public static partial class VmAQuake
{
    public const int QfAffectactors = 1 << 5;
    public const int QfDamagefalloff = 1 << 6;
    public const int QfGroundonly = 1 << 7;
    public const int Qf3d = 1 << 8;
    public const int QfWave = 1 << 9;
    public const int QfRelative = 1 << 10;
    public const int CfNoclip = 1 << 0;
    public const int ChanBody = 0;

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

    public sealed partial class QuakeThinker
    {
        public object QuakeSfx { get; set; } = new();
        public double IntensityZ { get; set; }
        public double WaveSpeedX { get; set; }
        public double WaveSpeedY { get; set; }
        public double WaveSpeedZ { get; set; }
        public double RollWave { get; set; }
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
                    DoQuakeDamage(quake, victim, falloff, ctx.RandomQuake, ctx.HitDice);
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
}

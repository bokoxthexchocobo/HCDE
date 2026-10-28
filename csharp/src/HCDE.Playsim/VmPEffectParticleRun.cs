namespace HCDE.Playsim;

/// <summary>Faithful port of <c>P_ThinkParticles</c>, <c>P_SpawnParticle</c>, <c>P_RunEffect</c> (<c>p_effect.cpp</c> 278–567).</summary>
public static class VmPEffectParticleRun
{
    public const int SpfNotimefreeze = 1;
    public const int SpfLocalAnim = 1 << 1;
    public const int SpfFadeInOut = 1 << 2;
    public const int SpfFadeInHoldOut = 1 << 3;
    public const int SpfNegativeFadeStep = 1 << 4;
    public const int SpfReplace = 1 << 5;
    public const int SpfRoll = 1 << 6;
    public const int FxRocket = 1;
    public const int FxGrenade = 1 << 1;
    public const int FxRespawninvul = 1 << 2;
    public const int NoParticle = -1;

    public sealed class Particle
    {
        public int Tnext { get; set; } = NoParticle;
        public int Flags { get; set; }
        public float Alpha { get; set; }
        public float FadeStep { get; set; }
        public float FadeOutStep { get; set; }
        public float Size { get; set; }
        public float SizeStep { get; set; }
        public int Ttl { get; set; }
        public double PosX { get; set; }
        public double PosY { get; set; }
        public double PosZ { get; set; }
        public double VelX { get; set; }
        public double VelY { get; set; }
        public double VelZ { get; set; }
        public double AccX { get; set; }
        public double AccY { get; set; }
        public double AccZ { get; set; }
        public float Roll { get; set; }
        public float RollVel { get; set; }
        public float RollAcc { get; set; }
        public object? Subsector { get; set; }
        public int AnimSwitchTic { get; set; }
        public int Color { get; set; }
    }

    public sealed class LevelParticles
    {
        public int ActiveParticles { get; set; } = NoParticle;
        public List<Particle> Particles { get; } = new();
        public bool Frozen { get; set; }
        public Func<double, double, double, double, (double X, double Y)>? PortalOffset { get; init; }
        public Func<double, double, double, object?>? PointInRenderSubsector { get; init; }
        public Func<object, bool>? PortalBlocksCeiling { get; init; }
        public Func<object, bool>? PortalBlocksFloor { get; init; }
        public Func<object, double>? PortalCeilingZ { get; init; }
        public Func<object, (double X, double Y, double Z)>? PortalCeilingDisplacement { get; init; }
        public Func<object, double>? PortalFloorZ { get; init; }
        public Func<object, (double X, double Y, double Z)>? PortalFloorDisplacement { get; init; }
    }

    public static void ThinkParticles(LevelParticles level)
    {
        var i = level.ActiveParticles;
        while (i != NoParticle)
        {
            var particle = level.Particles[i];
            i = particle.Tnext;
            if (level.Frozen && (particle.Flags & SpfNotimefreeze) == 0)
            {
                if ((particle.Flags & SpfLocalAnim) != 0) particle.AnimSwitchTic++;
                continue;
            }

            particle.Alpha -= particle.FadeStep;
            if (((particle.Flags & SpfFadeInOut) != 0 && particle.Alpha >= 1.0) ||
                ((particle.Flags & SpfFadeInHoldOut) != 0 &&
                 particle.Ttl * Math.Abs(particle.FadeOutStep) <= Math.Min(1.0f, Math.Abs(particle.Alpha))))
            {
                particle.Alpha = Math.Min(1.0f, Math.Abs(particle.Alpha));
                particle.FadeStep = particle.FadeOutStep;
                particle.Flags &= ~(SpfFadeInOut | SpfFadeInHoldOut);
            }

            if (particle.Alpha > 1.0f) particle.Alpha = 1.0f;
            particle.Size += particle.SizeStep;
            if (particle.Alpha <= 0 || --particle.Ttl <= 0 || particle.Size <= 0)
            {
                FreeParticle(level, particle);
                continue;
            }

            var newxy = level.PortalOffset?.Invoke(particle.PosX, particle.PosY, particle.VelX, particle.VelY)
                        ?? (particle.PosX + particle.VelX, particle.PosY + particle.VelY);
            particle.PosX = newxy.X;
            particle.PosY = newxy.Y;
            particle.PosZ += particle.VelZ;
            particle.VelX += particle.AccX;
            particle.VelY += particle.AccY;
            particle.VelZ += particle.AccZ;
            if ((particle.Flags & SpfRoll) != 0)
            {
                particle.Roll += particle.RollVel;
                particle.RollVel += particle.RollAcc;
            }

            var sub = level.PointInRenderSubsector?.Invoke(particle.PosX, particle.PosY, particle.PosZ);
            particle.Subsector = sub;
            if (sub == null) continue;
            if (level.PortalBlocksCeiling?.Invoke(sub) == false &&
                particle.PosZ > (level.PortalCeilingZ?.Invoke(sub) ?? double.MaxValue))
            {
                var d = level.PortalCeilingDisplacement?.Invoke(sub) ?? (0, 0, 0);
                particle.PosX += d.X; particle.PosY += d.Y; particle.PosZ += d.Z;
                particle.Subsector = null;
            }
            else if (level.PortalBlocksFloor?.Invoke(sub) == false &&
                     particle.PosZ < (level.PortalFloorZ?.Invoke(sub) ?? double.MinValue))
            {
                var d = level.PortalFloorDisplacement?.Invoke(sub) ?? (0, 0, 0);
                particle.PosX += d.X; particle.PosY += d.Y; particle.PosZ += d.Z;
                particle.Subsector = null;
            }
        }
    }

    public static void FreeParticle(LevelParticles level, Particle particle)
    {
        var idx = level.Particles.IndexOf(particle);
        if (idx < 0) return;
        particle.Tnext = level.ActiveParticles;
        level.ActiveParticles = idx;
    }

    public static float FadeFromTtl(int ttl) => ttl > 0 ? 1f / ttl : 0f;

    public static Particle? SpawnParticle(
        LevelParticles level, double posX, double posY, double posZ,
        double velX, double velY, double velZ, double accX, double accY, double accZ,
        int color, double startAlpha, int lifetime, double size, double fadeStep, double sizeStep,
        int flags, object? texture, object? style, double startRoll, double rollVel, double rollAcc,
        double fadeOutStep, Func<int, bool, Particle?>? newParticle = null)
    {
        var particle = newParticle?.Invoke(level.Particles.Count, (flags & SpfReplace) != 0) ?? new Particle();
        level.Particles.Add(particle);
        particle.PosX = posX; particle.PosY = posY; particle.PosZ = posZ;
        particle.VelX = velX; particle.VelY = velY; particle.VelZ = velZ;
        particle.AccX = accX; particle.AccY = accY; particle.AccZ = accZ;
        particle.Color = color;
        particle.Alpha = (float)startAlpha;
        if ((flags & (SpfFadeInOut | SpfFadeInHoldOut)) != 0) particle.FadeOutStep = (float)fadeOutStep;
        if ((fadeStep < 0 && (flags & SpfNegativeFadeStep) == 0) || fadeStep <= -1.0)
        {
            particle.FadeStep = FadeFromTtl(lifetime);
            if ((flags & SpfFadeInOut) != 0) particle.FadeStep *= -2;
            else if ((flags & SpfFadeInHoldOut) != 0) particle.FadeStep *= -3;
            particle.FadeOutStep = -particle.FadeStep;
        }
        else particle.FadeStep = (float)fadeStep;
        particle.Ttl = lifetime;
        particle.Size = (float)size;
        particle.SizeStep = (float)sizeStep;
        particle.Roll = (float)startRoll;
        particle.RollVel = (float)rollVel;
        particle.RollAcc = (float)rollAcc;
        particle.Flags = flags;
        return particle;
    }

    public sealed class RunEffectState
    {
        public int Effects { get; set; }
        public int ClRocketTrails { get; set; } = 1;
        public double VelX { get; set; }
        public double VelY { get; set; }
        public double VelZ { get; set; }
        public double Radius { get; set; }
        public double Height { get; set; }
        public int FountainColor { get; set; }
        public int LevelTime { get; set; }
        public List<Particle> Spawned { get; } = new();
        public Func<int, double, Particle?>? JitterParticle { get; init; }
        public Action<int, (double X, double Y, double Z), double, int, int>? DrawSplash2 { get; init; }
        public Func<double, double, double, (double X, double Y, double Z)>? Vec3Offset { get; init; }
        public Func<double, double, double, (double X, double Y, double Z)>? Vec3Angle { get; init; }
        public Func<int, int>? Random { get; init; }
    }

    public static void RunEffect(RunEffectState actor)
    {
        var moveangle = Math.Atan2(actor.VelY, actor.VelX);
        if ((actor.Effects & FxRocket) != 0 && (actor.ClRocketTrails & 1) != 0)
        {
            var backx = -actor.Radius * 2 * Math.Cos(moveangle);
            var backy = -actor.Radius * 2 * Math.Sin(moveangle);
            var backz = actor.Height * (2.0 / 3 - actor.VelZ / 8);
            var an = moveangle + Math.PI / 2;
            var p = actor.JitterParticle?.Invoke(3 + ((actor.Random?.Invoke(0) ?? 0) & 31), 1);
            if (p != null)
            {
                var pathdist = (actor.Random?.Invoke(0) ?? 0) / 256.0;
                var pos = actor.Vec3Offset?.Invoke(backx - actor.VelX * pathdist, backy - actor.VelY * pathdist, backz - actor.VelZ * pathdist) ?? (0, 0, 0);
                p.PosX = pos.X; p.PosY = pos.Y; p.PosZ = pos.Z;
                actor.Spawned.Add(p);
            }

            for (var i = 6; i > 0; i--)
            {
                var particle = actor.JitterParticle?.Invoke(3 + ((actor.Random?.Invoke(0) ?? 0) & 31), 1);
                if (particle == null) break;
                actor.Spawned.Add(particle);
            }
        }

        if ((actor.Effects & FxGrenade) != 0 && (actor.ClRocketTrails & 1) != 0)
        {
            var pos = actor.Vec3Angle?.Invoke(-actor.Radius * 2, moveangle, -actor.Height * actor.VelZ / 8 + actor.Height * (2.0 / 3)) ?? (0, 0, 0);
            actor.DrawSplash2?.Invoke(6, pos, moveangle + Math.PI, 2, 2);
        }

        if (actor.FountainColor != 0 && (actor.LevelTime & 1) == 0 &&
            actor.JitterParticle?.Invoke(51, 1) is { } fountain)
            actor.Spawned.Add(fountain);

        if ((actor.Effects & FxRespawninvul) != 0)
        {
            for (var i = 3; i > 0; i--)
                if (actor.JitterParticle?.Invoke(16, 1) is { } rp)
                    actor.Spawned.Add(rp);
        }
    }
}

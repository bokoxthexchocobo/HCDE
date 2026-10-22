namespace HCDE.Playsim;

/// <summary><c>JitterParticle</c>, <c>MakeFountain</c>, and splash spawners from <c>p_effect.cpp</c> (401–660).</summary>
public static class VmPEffectJitterSplash
{
    public sealed class SplashColors
    {
        public int Orange { get; set; }
        public int Yorange { get; set; }
        public int Blood1 { get; set; }
        public int Blood2 { get; set; }
        public int Grey3 { get; set; }
        public int Grey5 { get; set; }
        public int Grey1 { get; set; }
        public Func<int, int, int, int>? ParticleColor { get; init; }
        public Func<int, int>? ParticleColorRgb { get; init; }
    }

    public sealed class SplashLevel
    {
        public VmPEffectParticlePool.LevelParticles Particles { get; } = new();
        public Func<int, int>? Random { get; init; }
    }

    public sealed class FountainActor
    {
        public SplashLevel Level { get; init; } = null!;
        public double Radius { get; set; }
        public double Height { get; set; }
        public int LevelTime { get; set; }
        public Func<double, double, double, (double X, double Y, double Z)>? Vec3Angle { get; init; }
    }

    public static VmPEffectParticlePool.Particle? JitterParticle(SplashLevel level, int ttl) =>
        JitterParticle(level, ttl, 1.0);

    public static VmPEffectParticlePool.Particle? JitterParticle(SplashLevel level, int ttl, double drift)
    {
        var particle = VmPEffectParticlePool.NewParticle(level.Particles);
        if (particle is null)
            return null;

        for (var i = 3; i > 0; i--)
        {
            var r = (level.Random?.Invoke(0) ?? 0) - 128;
            var v = (1.0 / 4096) * r * drift;
            if (i == 1) particle.VelX = v;
            else if (i == 2) particle.VelY = v;
            else particle.VelZ = v;
        }

        for (var i = 3; i > 0; i--)
        {
            var r = (level.Random?.Invoke(0) ?? 0) - 128;
            var a = (1.0 / 16384) * r * drift;
            if (i == 1) particle.AccX = a;
            else if (i == 2) particle.AccY = a;
            else particle.AccZ = a;
        }

        particle.Alpha = 1f;
        particle.Ttl = ttl;
        particle.FadeStep = VmPEffectParticlePool.FadeFromTtl(ttl);
        return particle;
    }

    public static void MakeFountain(FountainActor actor, int color1, int color2)
    {
        if ((actor.LevelTime & 1) != 0)
            return;

        var particle = JitterParticle(actor.Level, 51);
        if (particle is null)
            return;

        var an = (actor.Level.Random?.Invoke(0) ?? 0) * (360.0 / 256) * Math.PI / 180;
        var outDist = actor.Radius * (actor.Level.Random?.Invoke(0) ?? 0) / 256.0;
        var pos = actor.Vec3Angle?.Invoke(outDist, an, actor.Height + 1) ?? (0, 0, 0);
        particle.PosX = pos.X;
        particle.PosY = pos.Y;
        particle.PosZ = pos.Z;
        if (outDist < actor.Radius / 8)
            particle.VelZ += 10f / 3;
        else
            particle.VelZ += 3;
        particle.AccZ -= 1f / 11;
        if ((actor.Level.Random?.Invoke(0) ?? 0) < 30)
        {
            particle.Size = 4;
            particle.Color = color2;
        }
        else
        {
            particle.Size = 6;
            particle.Color = color1;
        }
    }

    public static void DrawSplash(SplashLevel level, int count, double posX, double posY, double posZ, double angleRad, int kind, SplashColors colors)
    {
        int color1, color2;
        switch (kind)
        {
            case 1:
                color1 = colors.Orange;
                color2 = colors.Yorange;
                break;
            default:
                return;
        }

        for (; count > 0; count--)
        {
            var p = JitterParticle(level, 10);
            if (p is null)
                break;

            p.Size = 2;
            p.Color = ((level.Random?.Invoke(0) ?? 0) & 0x80) != 0 ? color1 : color2;
            p.VelZ -= (level.Random?.Invoke(0) ?? 0) / 128.0;
            p.AccZ -= 1.0 / 8;
            p.AccX += ((level.Random?.Invoke(0) ?? 0) - 128) / 8192.0;
            p.AccY += ((level.Random?.Invoke(0) ?? 0) - 128) / 8192.0;
            p.PosZ = posZ - (level.Random?.Invoke(0) ?? 0) / 64.0;
            angleRad += (level.Random?.Invoke(0) ?? 0) * (45.0 / 256) * Math.PI / 180;
            p.PosX = posX + ((level.Random?.Invoke(0) ?? 0) & 15) * Math.Cos(angleRad);
            p.PosY = posY + ((level.Random?.Invoke(0) ?? 0) & 15) * Math.Sin(angleRad);
        }
    }

    public static void DrawSplash2(
        SplashLevel level, int count, double posX, double posY, double posZ, double angleRad, int updown, int kind, SplashColors colors)
    {
        int color1, color2, zadd;
        double zvel, zspread;
        switch (kind)
        {
            case 0:
                color1 = colors.Blood1;
                color2 = colors.Blood2;
                break;
            case 1:
                color1 = colors.Grey3;
                color2 = colors.Grey5;
                break;
            case 2:
                color1 = colors.Grey3;
                color2 = colors.Grey1;
                break;
            default:
                color1 = colors.ParticleColorRgb?.Invoke(kind) ?? kind;
                color2 = colors.ParticleColor?.Invoke((kind & 0xff) / 3, ((kind >> 8) & 0xff) / 3, ((kind >> 16) & 0xff) / 3) ?? kind;
                break;
        }

        zvel = -1.0 / 512;
        zspread = updown != 0 ? -6000.0 / 65536 : 6000.0 / 65536;
        zadd = updown == 2 ? -128 : 0;

        for (; count > 0; count--)
        {
            var p = VmPEffectParticlePool.NewParticle(level.Particles);
            if (p is null)
                break;

            p.Ttl = 12;
            p.FadeStep = VmPEffectParticlePool.FadeFromTtl(12);
            p.Alpha = 1f;
            p.Size = 4;
            p.Color = ((level.Random?.Invoke(0) ?? 0) & 0x80) != 0 ? color1 : color2;
            p.VelZ = (level.Random?.Invoke(0) ?? 0) * zvel;
            p.AccZ = -1 / 22.0;
            if (kind != 0)
            {
                var an = angleRad + ((level.Random?.Invoke(0) ?? 0) - 128) * (180.0 / 256) * Math.PI / 180;
                p.VelX = (level.Random?.Invoke(0) ?? 0) * Math.Cos(an) / 2048.0;
                p.VelY = (level.Random?.Invoke(0) ?? 0) * Math.Sin(an) / 2048.0;
                p.AccX = p.VelX / 16;
                p.AccY = p.VelY / 16;
            }

            var an2 = angleRad + ((level.Random?.Invoke(0) ?? 0) - 128) * (90.0 / 256) * Math.PI / 180;
            p.PosX = posX + (((level.Random?.Invoke(0) ?? 0) & 31) - 15) * Math.Cos(an2);
            p.PosY = posY + (((level.Random?.Invoke(0) ?? 0) & 31) - 15) * Math.Sin(an2);
            p.PosZ = posZ + ((level.Random?.Invoke(0) ?? 0) + zadd - 128) * zspread;
        }
    }
}

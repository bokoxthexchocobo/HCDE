namespace HCDE.Playsim;

public static class VmPEffectParticlePool
{
    public const int NoParticle = 0xffff;
    public const int SpfLocalAnim = 1 << 1;

    public sealed class Particle
    {
        public int Tprev { get; set; } = NoParticle;
        public int Tnext { get; set; } = NoParticle;
        public int Snext { get; set; } = NoParticle;
        public double PosX { get; set; }
        public double PosY { get; set; }
        public double PosZ { get; set; }
        public double VelX { get; set; }
        public double VelY { get; set; }
        public double VelZ { get; set; }
        public double AccX { get; set; }
        public double AccY { get; set; }
        public double AccZ { get; set; }
        public float Alpha { get; set; }
        public int Ttl { get; set; }
        public float FadeStep { get; set; }
        public float Size { get; set; }
        public int Color { get; set; }
        public object? Subsector { get; set; }
    }

    public sealed class SubsectorBucket
    {
        public List<object> Sprites { get; } = new();
        public int Index { get; init; }
    }

    public sealed class VisualThinkerNode
    {
        public double PosX { get; set; }
        public double PosY { get; set; }
        public object? Subsector { get; set; }
        public VisualThinkerNode? Next { get; set; }
    }

    public sealed class LevelParticles
    {
        public int OldestParticle { get; set; } = NoParticle;
        public int ActiveParticles { get; set; } = NoParticle;
        public int InactiveParticles { get; set; }
        public List<Particle> Particles { get; } = new();
        public List<ushort> ParticlesInSubsec { get; } = new();
        public List<SubsectorBucket> Subsectors { get; } = new();
        public VisualThinkerNode? VisualThinkerHead { get; set; }
        public bool Frozen { get; set; }
        public Func<double, double, object?>? PointInRenderSubsector { get; init; }
    }

    public sealed class InitParticlesArgs
    {
        public string? NumParticlesArg { get; init; }
        public int RMaxParticles { get; init; } = 4000;
    }

    public sealed class InitEffectsArgs
    {
        public Func<int, int, int, int>? ColorMatcherPick { get; init; }
        public int DefaultBloodParticleColor { get; init; }
        public Dictionary<string, int> NamedColors { get; } = new();
        public int Blood1 { get; set; }
        public int Blood2 { get; set; }
    }

    private static readonly Dictionary<int, int> ColorSaver = new();

    public static void FreeParticle(LevelParticles level, int pindex)
    {
        var particle = level.Particles[pindex];
        var prev = particle.Tprev == NoParticle ? null : level.Particles[particle.Tprev];
        var tnext = particle.Tnext;
        if (prev is not null)
            prev.Tnext = tnext;
        else
            level.ActiveParticles = tnext;

        if (tnext != NoParticle)
        {
            var next = level.Particles[tnext];
            next.Tprev = particle.Tprev;
        }

        if (level.OldestParticle == pindex)
            level.OldestParticle = particle.Tprev;

        level.Particles[pindex] = new Particle();
        particle = level.Particles[pindex];
        particle.Tnext = level.InactiveParticles;
        level.InactiveParticles = pindex;
    }

    public static Particle? NewParticle(LevelParticles level, bool replace = false)
    {
        if (level.InactiveParticles == NoParticle && level.OldestParticle != NoParticle)
        {
            if (!replace)
                return null;
            FreeParticle(level, level.OldestParticle);
        }

        var current = level.ActiveParticles;
        var inactive = level.InactiveParticles;
        var result = level.Particles[inactive];
        level.InactiveParticles = result.Tnext;
        result.Tnext = current;
        result.Tprev = NoParticle;
        var resultIndex = inactive;
        level.ActiveParticles = resultIndex;

        if (current != NoParticle)
        {
            var next = level.Particles[current];
            next.Tprev = level.ActiveParticles;
        }
        else
            level.OldestParticle = level.ActiveParticles;

        return result;
    }

    public static void InitParticles(LevelParticles level, InitParticlesArgs args)
    {
        var num = args.NumParticlesArg is not null
            ? int.Parse(args.NumParticlesArg)
            : args.RMaxParticles;
        var count = Math.Clamp(num, 100, 65535);
        level.Particles.Clear();
        for (var i = 0; i < count; i++)
            level.Particles.Add(new Particle());
        ClearParticles(level);
    }

    public static void ClearParticles(LevelParticles level)
    {
        var i = 0;
        level.OldestParticle = NoParticle;
        level.ActiveParticles = NoParticle;
        level.InactiveParticles = 0;
        foreach (var p in level.Particles)
        {
            p.Tprev = i == 0 ? NoParticle : i - 1;
            p.Tnext = ++i;
        }

        if (level.Particles.Count > 0)
            level.Particles[^1].Tnext = NoParticle;
    }

    public static void FindParticleSubsectors(LevelParticles level, bool rParticles)
    {
        foreach (var ss in level.Subsectors)
            ss.Sprites.Clear();

        for (var sp = level.VisualThinkerHead; sp is not null; sp = sp.Next)
        {
            if (sp.Subsector is null)
                sp.Subsector = level.PointInRenderSubsector?.Invoke(sp.PosX, sp.PosY);
            if (sp.Subsector is SubsectorBucket bucket)
                bucket.Sprites.Add(sp);
        }

        while (level.ParticlesInSubsec.Count < level.Subsectors.Count)
            level.ParticlesInSubsec.Add(NoParticle);

        for (var i = 0; i < level.ParticlesInSubsec.Count; i++)
            level.ParticlesInSubsec[i] = NoParticle;

        if (!rParticles)
            return;

        for (var i = level.ActiveParticles; i != NoParticle; i = level.Particles[i].Tnext)
        {
            var particle = level.Particles[i];
            if (particle.Subsector is null)
                particle.Subsector = level.PointInRenderSubsector?.Invoke(particle.PosX, particle.PosY);
            if (particle.Subsector is not SubsectorBucket ss)
                continue;
            var ssnum = ss.Index;
            particle.Snext = level.ParticlesInSubsec[ssnum];
            level.ParticlesInSubsec[ssnum] = (ushort)i;
        }
    }

    public static int ParticleColorRgb(int rgb, Func<int, int, int, int>? pick)
    {
        if (ColorSaver.TryGetValue(rgb, out var cached))
            return cached;
        var r = rgb & 0xff;
        var g = (rgb >> 8) & 0xff;
        var b = (rgb >> 16) & 0xff;
        var stuff = rgb | ((pick?.Invoke(r, g, b) ?? 0) << 24);
        ColorSaver[rgb] = stuff;
        return stuff;
    }

    public static int ParticleColor(int r, int g, int b, Func<int, int, int, int>? pick) =>
        ParticleColorRgb(r | (g << 8) | (b << 16), pick);

    public static void InitEffects(InitEffectsArgs args)
    {
        var colors = new (string name, byte r, byte g, byte b)[]
        {
            ("grey1", 85, 85, 85),
            ("grey2", 171, 171, 171),
            ("grey3", 50, 50, 50),
            ("grey4", 210, 210, 210),
            ("grey5", 128, 128, 128),
            ("red", 255, 0, 0),
            ("green", 0, 200, 0),
            ("blue", 0, 0, 255),
            ("yellow", 255, 255, 0),
            ("black", 0, 0, 0),
            ("red1", 255, 127, 127),
            ("green1", 127, 255, 127),
            ("blue1", 127, 127, 255),
            ("yellow1", 255, 255, 180),
            ("purple", 120, 0, 160),
            ("purple1", 200, 30, 255),
            ("white", 255, 255, 255),
            ("rblue1", 81, 81, 255),
            ("rblue2", 0, 0, 227),
            ("rblue3", 0, 0, 130),
            ("rblue4", 0, 0, 80),
            ("orange", 255, 120, 0),
            ("yorange", 255, 170, 0),
            ("dred", 80, 0, 0),
            ("maroon1", 154, 49, 49),
            ("maroon2", 125, 24, 24),
        };

        foreach (var (name, r, g, b) in colors)
            args.NamedColors[name] = ParticleColor(r, g, b, args.ColorMatcherPick);

        var kind = args.DefaultBloodParticleColor;
        args.Blood1 = ParticleColorRgb(kind, args.ColorMatcherPick);
        args.Blood2 = ParticleColor(
            (kind & 0xff) / 3,
            ((kind >> 8) & 0xff) / 3,
            ((kind >> 16) & 0xff) / 3,
            args.ColorMatcherPick);
    }

}

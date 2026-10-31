namespace HCDE.Playsim;

/// <summary>
/// <c>P_ExplodeMissile</c> policy from <c>p_mobj.cpp</c> (**1951–2100**). Chunk **1110**.
/// </summary>
public static class VmPMobjExplodeMissile1110Slice
{
    public const int Mf3ExploCount = 0x1;
    public const int MfShootable = 0x4;
    public const int Mf3SkyExplode = 0x2;
    public const int Mf7HitTarget = 0x10;
    public const int Mf7HitMaster = 0x20;
    public const int Mf7HitTracer = 0x40;
    public const int MfNoBlood = 0x8;
    public const int MfCorpse = 0x10;
    public const int Mf6Killed = 0x1;
    public const int LineHorizon = 0x7D;
    public const int OfEuthanizeMe = 0x1;
    public const int Mf5DehExplosion = 0x2;
    public const int Mf4Randomize = 0x4;
    public const int MfMissile = 0x100;
    public const int StyleAdd = 1;
    public const int StyleTranslucent = 2;

    public sealed class ExplodeMissileContext
    {
        public bool HasLine { get; init; }
        public int LineSpecial { get; init; }
        public bool OnSky { get; init; }
        public int Flags3 { get; set; }
        public int Threshold { get; set; }
        public int DefThreshold { get; init; }
        public int Flags { get; set; }
        public int Flags4 { get; init; }
        public int Flags5 { get; init; }
        public int Flags7 { get; init; }
        public bool HasTarget { get; init; }
        public int TargetFlags { get; init; }
        public int TargetFlags6 { get; init; }
        public bool AssignTarget { get; set; }
        public bool AssignMaster { get; set; }
        public bool AssignTracer { get; set; }
        public bool UseCrashState { get; set; }
        public bool UseExtremeDeath { get; set; }
        public bool DestroyOnSky { get; set; }
        public bool UseSkyDeath { get; set; }
        public bool MissileDecals { get; init; }
        public bool HasDecal { get; init; }
        public double Den { get; init; }
        public double RawFrac { get; init; }
        public bool PlayDeathSound { get; set; }
        public bool FullVolumeDeath { get; init; }
        public int ObjectFlags { get; init; }
        public int ExplosionStyle { get; init; }
        public bool AddRocketExplosion { get; init; }
        public int RenderStyle { get; set; }
        public double Alpha { get; set; }
        public int Tics { get; set; }
        public int RandomMask { get; init; }
        public Action? HitLinedef { get; init; }
        public bool Aborted { get; set; }
    }

    public static void RunExplodeMissile(ExplodeMissileContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        if (ctx.HasLine)
            ctx.HitLinedef?.Invoke();

        if ((ctx.Flags3 & Mf3ExploCount) != 0)
        {
            ctx.Threshold++;
            if (ctx.Threshold < ctx.DefThreshold)
            {
                ctx.Aborted = true;
                return;
            }
        }

        ctx.Flags &= ~MfShootable;
        if (ctx.HasTarget)
        {
            if ((ctx.Flags7 & Mf7HitTarget) != 0) ctx.AssignTarget = true;
            if ((ctx.Flags7 & Mf7HitMaster) != 0) ctx.AssignMaster = true;
            if ((ctx.Flags7 & Mf7HitTracer) != 0) ctx.AssignTracer = true;
            if ((ctx.TargetFlags & (MfShootable | MfCorpse)) != 0 || (ctx.TargetFlags6 & Mf6Killed) != 0)
            {
                if ((ctx.TargetFlags & MfNoBlood) != 0)
                    ctx.UseCrashState = true;
                ctx.UseExtremeDeath = !ctx.UseCrashState;
            }
        }

        if (ctx.OnSky || (ctx.HasLine && ctx.LineSpecial == LineHorizon))
        {
            if ((ctx.Flags3 & Mf3SkyExplode) == 0)
            {
                ctx.DestroyOnSky = true;
                ctx.Aborted = true;
                return;
            }

            ctx.UseSkyDeath = true;
        }

        ctx.PlayDeathSound = true;
        if ((ctx.ObjectFlags & OfEuthanizeMe) != 0)
            return;

        if ((ctx.Flags5 & Mf5DehExplosion) != 0)
        {
            if (ctx.ExplosionStyle == 255)
            {
                if (ctx.AddRocketExplosion)
                {
                    ctx.RenderStyle = StyleAdd;
                    ctx.Alpha = 1.0;
                }
                else
                {
                    ctx.RenderStyle = StyleTranslucent;
                    ctx.Alpha = 0.6666;
                }
            }
            else
            {
                ctx.RenderStyle = ctx.ExplosionStyle;
                ctx.Alpha = ctx.Alpha;
            }
        }

        if ((ctx.Flags4 & Mf4Randomize) != 0)
        {
            ctx.Tics -= (ctx.RandomMask & 3) * 35 / 35;
            if (ctx.Tics < 1)
                ctx.Tics = 1;
        }

        ctx.Flags &= ~MfMissile;
    }

    public static double ClampDecalFraction(double raw, double den)
    {
        if (den == 0)
            return 0;
        var frac = raw / den;
        if (frac < 0) return 0;
        if (frac > 1) return 1;
        return frac;
    }

    public static int FlipMissingSide(int side, bool sideDefPresent) =>
        sideDefPresent ? side : side ^ 1;
}

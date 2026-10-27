namespace HCDE.Playsim;

/// <summary>Static <c>Grind</c> and <c>CallGrind</c> from <c>p_mobj.cpp</c> (1554–1705).</summary>
public static class VmPMobjGrind
{
    public const int MfCorpse = 1 << 0;
    public const int MfSolid = 1 << 1;
    public const int MfShootable = 1 << 2;
    public const int MfNoblood = 1 << 3;
    public const int MfIcecorpse = 1 << 4;
    public const int MfNoclip = 1 << 5;
    public const int Mf3Dontgib = 1 << 0;
    public const int Mf4Bossdeath = 1 << 0;
    public const int Mf6Touchy = 1 << 0;
    public const int Mf6Armed = 1 << 1;
    public const int RfInvisible = 1;
    public const int DmgForced = 1;

    public sealed class GrindActor
    {
        public int Flags { get; set; }
        public int Flags3 { get; set; }
        public int Flags4 { get; set; }
        public int Flags6 { get; set; }
        public int Health { get; set; }
        public bool HasPlayer { get; set; }
        public bool DontCrunchCorpses { get; set; }
        public object? CrushState { get; set; }
        public object? GenericCrushState { get; set; }
        public double Height { get; set; }
        public double Radius { get; set; }
        public int Tics { get; set; }
        public int RenderFlags { get; set; }
        public object? BloodTranslation { get; set; }
        public object? Translation { get; set; }
        public bool VelZeroed { get; set; }
        public bool Destroyed { get; set; }
        public bool IsSentient { get; set; }
    }

    public sealed class GrindContext
    {
        public bool Items { get; init; }
        public Func<GrindActor, object?>? FindCrushState { get; init; }
        public Func<GrindActor, object?>? FindGenericCrushState { get; init; }
        public Func<object?, int>? SpriteNumFrames { get; init; }
        public Func<object?>? FindRealGibs { get; init; }
        public Func<object?, object?>? GetReplacement { get; init; }
        public Func<object?, bool>? GibClassHasSpawnFrames { get; init; }
        public Func<object?, object?>? SpawnGib { get; init; }
        public Action<object?, object?, object?, double, double, object?>? ApplyGibAppearance { get; init; }
        public Action<object?, object?>? BossDeath { get; init; }
        public Action<object?, object?>? SetState { get; init; }
        public Action<object?, object?, object?>? WorldThingGround { get; init; }
        public Action<object?, object?, object?, int, int>? PlaySound { get; init; }
        public Action<object?, object?, object?, int, int, int>? DamageMobj { get; init; }
    }

    public static void ZeroCorpseBounds(GrindActor actor)
    {
        actor.Height = 0;
        actor.Radius = 0;
        actor.VelZeroed = true;
    }

    public static object? PickGenericCrushState(GrindContext ctx, GrindActor actor)
    {
        if ((actor.Flags & MfNoblood) != 0 || actor.HasPlayer)
            return null;
        var generic = ctx.FindGenericCrushState?.Invoke(actor) ?? actor.GenericCrushState;
        if (generic == null)
            return null;
        var frames = ctx.SpriteNumFrames?.Invoke(generic) ?? 1;
        return frames > 0 ? generic : null;
    }

    public static object? ResolveRealGibsClass(GrindContext ctx, GrindActor actor)
    {
        var cls = ctx.FindRealGibs?.Invoke();
        if (cls == null)
            return null;
        cls = ctx.GetReplacement?.Invoke(cls) ?? cls;
        if (ctx.GibClassHasSpawnFrames != null && !ctx.GibClassHasSpawnFrames(cls))
            return null;
        return cls;
    }

    public static bool Grind(GrindContext ctx, GrindActor actor)
    {
        if ((actor.Flags & MfCorpse) != 0 && (actor.Flags3 & Mf3Dontgib) == 0 && actor.Health <= 0)
        {
            var state = ctx.FindCrushState?.Invoke(actor) ?? actor.CrushState;
            if (state == null && actor.DontCrunchCorpses)
            {
                actor.Flags &= ~MfSolid;
                actor.Flags3 |= Mf3Dontgib;
                ZeroCorpseBounds(actor);
                return false;
            }

            var isGeneric = false;
            if (state == null && (actor.Flags & MfNoblood) == 0 && !actor.HasPlayer)
            {
                isGeneric = true;
                state = PickGenericCrushState(ctx, actor);
            }

            if (state != null && (actor.Flags & MfIcecorpse) == 0)
            {
                if ((actor.Flags4 & Mf4Bossdeath) != 0)
                    ctx.BossDeath?.Invoke(actor, actor);
                actor.Flags &= ~MfSolid;
                actor.Flags3 |= Mf3Dontgib;
                ZeroCorpseBounds(actor);
                ctx.SetState?.Invoke(actor, state);
                if (isGeneric)
                {
                    ctx.PlaySound?.Invoke(actor, actor, actor, 0, 0);
                    actor.Translation = actor.BloodTranslation;
                }
                ctx.WorldThingGround?.Invoke(actor, actor, state);
                return false;
            }

            if ((actor.Flags & MfNoblood) == 0)
            {
                if ((actor.Flags4 & Mf4Bossdeath) != 0)
                    ctx.BossDeath?.Invoke(actor, actor);
                var gibClass = ResolveRealGibsClass(ctx, actor);
                if (gibClass == null)
                {
                    actor.Flags &= ~MfSolid;
                    actor.Flags3 |= Mf3Dontgib;
                    ZeroCorpseBounds(actor);
                    return false;
                }

                var gib = ctx.SpawnGib?.Invoke(actor);
                if (gib != null)
                    ctx.ApplyGibAppearance?.Invoke(actor, gib, gibClass, actor.Height, actor.Radius, actor.BloodTranslation);
                ctx.PlaySound?.Invoke(actor, actor, actor, 0, 0);
                ctx.WorldThingGround?.Invoke(actor, actor, null);
            }

            if ((actor.Flags & MfIcecorpse) != 0)
            {
                actor.Tics = 1;
                actor.VelZeroed = true;
            }
            else if (actor.HasPlayer)
            {
                actor.Flags |= MfNoclip;
                actor.Flags3 |= Mf3Dontgib;
                actor.RenderFlags |= RfInvisible;
            }
            else
                actor.Destroyed = true;
            return false;
        }

        if ((actor.Flags6 & Mf6Touchy) != 0 && ((actor.Flags6 & Mf6Armed) != 0 || actor.IsSentient))
        {
            actor.Flags6 &= ~Mf6Armed;
            ctx.DamageMobj?.Invoke(actor, actor, actor, actor.Health, 0, DmgForced);
            return true;
        }

        if ((actor.Flags & MfSolid) == 0 || (actor.Flags & MfNoclip) != 0)
            return false;
        if ((actor.Flags & MfShootable) == 0)
            return false;
        return true;
    }

    public static bool CallGrind(GrindContext ctx, GrindActor actor, bool hasVirtual, int vmReturn)
    {
        if (hasVirtual)
            return vmReturn != 0;
        return Grind(ctx, actor);
    }
}

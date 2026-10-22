namespace HCDE.Playsim;

/// <summary>Static <c>Grind</c>, action wrapper, and <c>CallGrind</c> from <c>p_mobj.cpp</c> (1554–1705).</summary>
public static class VmPMobjGrind
{
    public const int MfCorpse = 1;
    public const int MfSolid = 1;
    public const int MfShootable = 1;
    public const int MfNoblood = 1;
    public const int MfIcecorpse = 1;
    public const int Mf3Dontgib = 1;
    public const int Mf4Bossdeath = 1;
    public const int Mf6Touchy = 1;
    public const int Mf6Armed = 1;
    public const int MfNoclip = 1;
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
        public bool VelZeroed { get; set; }
        public bool Destroyed { get; set; }
        public bool IsSentient { get; set; }
    }

    public sealed class GrindContext
    {
        public bool Items { get; init; }
        public Func<object?>? FindRealGibs { get; init; }
        public Func<object?, object?>? SpawnGib { get; init; }
        public Action<object?, object?>? BossDeath { get; init; }
        public Action<object?, object?>? SetState { get; init; }
        public Action<object?, object?, object?>? WorldThingGround { get; init; }
        public Action<object?, object?, object?, int, int>? PlaySound { get; init; }
        public Action<object?, object?, object?, int, int, int>? DamageMobj { get; init; }
    }

    public static bool Grind(GrindContext ctx, GrindActor actor)
    {
        if ((actor.Flags & MfCorpse) != 0 && (actor.Flags3 & Mf3Dontgib) == 0 && actor.Health <= 0)
        {
            var state = actor.CrushState;
            if (state == null && actor.DontCrunchCorpses)
            {
                actor.Flags &= ~MfSolid;
                actor.Flags3 |= Mf3Dontgib;
                actor.VelZeroed = true;
                return false;
            }

            var isGeneric = false;
            if (state == null && (actor.Flags & MfNoblood) == 0 && !actor.HasPlayer)
            {
                isGeneric = true;
                state = actor.GenericCrushState;
            }

            if (state != null && (actor.Flags & MfIcecorpse) == 0)
            {
                if ((actor.Flags4 & Mf4Bossdeath) != 0)
                    ctx.BossDeath?.Invoke(actor, actor);
                actor.Flags &= ~MfSolid;
                actor.Flags3 |= Mf3Dontgib;
                actor.VelZeroed = true;
                ctx.SetState?.Invoke(actor, state);
                if (isGeneric)
                    ctx.PlaySound?.Invoke(actor, actor, actor, 0, 0);
                ctx.WorldThingGround?.Invoke(actor, actor, state);
                return false;
            }

            if ((actor.Flags & MfNoblood) == 0)
            {
                if ((actor.Flags4 & Mf4Bossdeath) != 0)
                    ctx.BossDeath?.Invoke(actor, actor);
                var gibType = ctx.FindRealGibs?.Invoke();
                if (gibType == null)
                {
                    actor.Flags &= ~MfSolid;
                    actor.Flags3 |= Mf3Dontgib;
                    actor.VelZeroed = true;
                    return false;
                }

                ctx.SpawnGib?.Invoke(actor);
                ctx.PlaySound?.Invoke(actor, actor, actor, 0, 0);
                ctx.WorldThingGround?.Invoke(actor, actor, null);
            }

            if ((actor.Flags & MfIcecorpse) != 0)
                actor.VelZeroed = true;
            else if (actor.HasPlayer)
            {
                actor.Flags |= MfNoclip;
                actor.Flags3 |= Mf3Dontgib;
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

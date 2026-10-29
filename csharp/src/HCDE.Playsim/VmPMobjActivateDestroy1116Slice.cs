namespace HCDE.Playsim;

/// <summary>
/// <c>PostBeginPlay</c>, fast/slow skill, activate/deactivate, and <c>OnDestroy</c> premorph cleanup from
/// <c>p_mobj.cpp</c> (**5827–5976**). Chunk **1116**.
/// </summary>
public static class VmPMobjActivateDestroy1116Slice
{
    public const int Mf7HandleNoDelay = 1;
    public const int Mf8RecreateLights = 2;
    public const int Level3LightCreated = 4;
    public const int Mf5AlwaysFast = 8;
    public const int Mf5NeverFast = 16;
    public const int Mf3IsMonster = 32;
    public const int MfIceCorpse = 64;
    public const int Mf2Dormant = 128;
    public const int MfUnmorphed = 256;

    public sealed class PostBeginContext
    {
        public int Flags7 { get; set; }
        public int Flags8 { get; set; }
        public int LevelFlags3 { get; set; }
        public bool HasLightAssociations { get; init; }
        public bool StateHasLight { get; init; }
    }

    public static void RunPostBeginPlay(PostBeginContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        ctx.Flags7 |= Mf7HandleNoDelay;
        if (ctx.HasLightAssociations || ctx.StateHasLight)
        {
            ctx.Flags8 |= Mf8RecreateLights;
            ctx.LevelFlags3 |= Level3LightCreated;
        }
    }

    public static bool IsFast(int flags5, bool skillFastMonsters)
    {
        if ((flags5 & Mf5AlwaysFast) != 0)
            return true;
        if ((flags5 & Mf5NeverFast) != 0)
            return false;
        return skillFastMonsters;
    }

    public sealed class ActivationContext
    {
        public int Flags { get; init; }
        public int Flags2 { get; set; }
        public int Flags3 { get; init; }
        public int Health { get; init; }
        public bool HasActiveState { get; init; }
        public bool HasInactiveState { get; init; }
        public int Tics { get; set; }
        public bool SetActive { get; set; }
        public bool SetInactive { get; set; }
    }

    public static void RunActivate(ActivationContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        if ((ctx.Flags3 & Mf3IsMonster) == 0)
            return;
        if (ctx.Health <= 0 && (ctx.Flags & MfIceCorpse) == 0)
            return;
        if ((ctx.Flags2 & Mf2Dormant) == 0)
            return;

        ctx.Flags2 &= ~Mf2Dormant;
        if (ctx.HasActiveState)
            ctx.SetActive = true;
        else
            ctx.Tics = 1;
    }

    public static void RunDeactivate(ActivationContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        if ((ctx.Flags3 & Mf3IsMonster) == 0)
            return;
        if (ctx.Health <= 0 && (ctx.Flags & MfIceCorpse) == 0)
            return;
        if ((ctx.Flags2 & Mf2Dormant) != 0)
            return;

        ctx.Flags2 |= Mf2Dormant;
        if (ctx.HasInactiveState)
            ctx.SetInactive = true;
        else
            ctx.Tics = -1;
    }

    public sealed class OnDestroyContext
    {
        public bool HasAlternative { get; init; }
        public int Flags { get; init; }
        public bool HasLevel { get; init; }
        public bool ClearAlternativeCounters { get; set; }
        public bool DestroyAlternative { get; set; }
        public bool WorldDestroyed { get; set; }
        public bool ClearBehaviors { get; set; }
    }

    public static void RunOnDestroyPremorph(OnDestroyContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        if (ctx.HasAlternative && (ctx.Flags & MfUnmorphed) == 0)
        {
            ctx.ClearAlternativeCounters = true;
            ctx.DestroyAlternative = true;
        }

        if (ctx.HasLevel)
            ctx.WorldDestroyed = true;
        ctx.ClearBehaviors = true;
    }
}

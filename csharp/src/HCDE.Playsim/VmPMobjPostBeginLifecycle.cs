namespace HCDE.Playsim;

/// <summary>Post-begin play, fast/slow, activate/deactivate, and destroy from <c>p_mobj.cpp</c> (5825–5998).</summary>
public static class VmPMobjPostBeginLifecycle
{
    public const int Mf7Handlenodelay = 1;
    public const int Mf8Recreatelights = 1;
    public const int Level3Lightcreated = 1;
    public const int Mf5Alwaysfast = 1;
    public const int Mf5Neverfast = 1 << 1;
    public const int Mf3Ismonster = 1;
    public const int MfIcecorpse = 1;
    public const int Mf2Dormant = 1;
    public const int MfUnmorphed = 1;

    public static int PostBeginPlayFlags(int flags7, bool hasLights) =>
        flags7 | Mf7Handlenodelay | (hasLights ? 0 : 0);

    public static bool ShouldRecreateLights(bool associations, bool stateLight) =>
        associations || stateLight;

    public static int ApplyRecreateLightsFlag(int flags8) => flags8 | Mf8Recreatelights;

    public static int ApplyLevelLightCreated(int levelFlags3) => levelFlags3 | Level3Lightcreated;

    public static bool IsFastMonster(int flags5, bool skillFast) =>
        (flags5 & Mf5Alwaysfast) != 0 || (flags5 & Mf5Neverfast) == 0 && skillFast;

public static bool ActivateEligible(int flags3, int health, int flags) =>
        (flags3 & Mf3Ismonster) != 0 && (health > 0 || (flags & MfIcecorpse) != 0);

    public static bool WakeFromDormant(int flags2) => (flags2 & Mf2Dormant) != 0;

    public static int ClearDormant(int flags2) => flags2 & ~Mf2Dormant;

    public static int SetDormant(int flags2) => flags2 | Mf2Dormant;

    public static int ActivateTicsWhenNoState() => 1;

    public static int DeactivateTicsWhenNoState() => -1;

    public static bool DeactivateEligible(int flags3, int health, int flags) =>
        ActivateEligible(flags3, health, flags);

    public static bool AlreadyDormant(int flags2) => (flags2 & Mf2Dormant) != 0;

    public readonly record struct OnDestroyPremorphContext(
        bool HasAlternative,
        int Flags,
        bool AlternativeDestroyed);

    public static bool ShouldDestroyPremorph(OnDestroyPremorphContext ctx) =>
        ctx.HasAlternative && (ctx.Flags & MfUnmorphed) == 0;

    public static bool WorldThingDestroyedWhenLevel(bool levelNull) => !levelNull;

    public static int OrphanSectorBlockmapFlags(int flags) => flags | (1 << 1) | (1 << 2);

public readonly record struct ActivateFlowContext(
        int Flags3,
        int Health,
        int Flags,
        int Flags2,
        bool HasActiveState);

    public static ActivateFlowResult EvaluateActivate(ActivateFlowContext ctx)
    {
        if (!ActivateEligible(ctx.Flags3, ctx.Health, ctx.Flags)) return ActivateFlowResult.NoOp;
        if (!WakeFromDormant(ctx.Flags2)) return ActivateFlowResult.NoOp;
        return ctx.HasActiveState ? ActivateFlowResult.SetState : ActivateFlowResult.SetTics;
    }

    public enum ActivateFlowResult { NoOp, SetState, SetTics }

    public static ActivateFlowResult EvaluateDeactivate(ActivateFlowContext ctx)
    {
        if (!DeactivateEligible(ctx.Flags3, ctx.Health, ctx.Flags)) return ActivateFlowResult.NoOp;
        if (AlreadyDormant(ctx.Flags2)) return ActivateFlowResult.NoOp;
        return ctx.HasActiveState ? ActivateFlowResult.SetState : ActivateFlowResult.SetTics;
    }
}

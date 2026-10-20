namespace HCDE.Playsim;

/// <summary><c>DamageMobj</c> reflection prelude, damage wrappers, and virtual target-switch glue from <c>p_interaction.cpp</c>.</summary>
public static class VmPInteractionVirtualDamage
{
    public const int NameReflection = 1;
    public const int DmgThrustless = 1;
    public const int MfNoclip = 1;
    public const int Mf2Nodmgthrust = 1;
    public const int Mf7Dontthrust = 1;
    public const int DmgForced = 1;

    public static bool ReflectionEligible(
        int damage,
        bool hasSource,
        int modReflection,
        int mod,
        bool targetNotSource) =>
        damage > 0 && hasSource && mod != modReflection && targetNotSource;

    public static bool PickMaxReflection(int candidate, int current) => candidate > current;

    public static bool ReflectDamagePositive(int reflectDamage) => reflectDamage > 0;

    public static bool UseReflectMod(bool reflectType, int mod, int reflectionMod) =>
        reflectType ? mod != 0 : true;

    public static bool KickbackEligible(
        bool plrDontThrust,
        bool hasInflictor,
        bool selfHit,
        bool noclip,
        bool inflictorNoThrust,
        int flags,
        int flags7,
        bool sourceNoThrust) =>
        !plrDontThrust && hasInflictor && !selfHit && !noclip
        && !inflictorNoThrust && (flags & DmgThrustless) == 0
        && (flags7 & Mf7Dontthrust) == 0 && !sourceNoThrust;

    public static bool MirrorActorSkips(bool t, bool i, bool s) => t || i || s;

    public static bool CoopVisualSkips(bool t, bool i, bool s) => t || i || s;

    public static bool DamagePositive(int d) => d > 0;

    public static bool RecordLagCompDamage(bool active, bool replaying) => active && !replaying;

    public static bool RecordNetAttack(bool hasSource, bool lagActive, bool replaying) =>
        hasSource && (!lagActive || replaying);

    public static bool VirtualDamageMobj(bool hasVirtual) => hasVirtual;

    public static bool VirtualOkaySwitch(bool hasVirtual) => hasVirtual;

    public static bool CallReturnsTrue(int ret) => ret != 0;

    public static bool FallbackOkaySwitch(bool hasVirtual) => !hasVirtual;

    public readonly record struct ReflectionScanContext(
        int Damage,
        bool HasSource,
        int Mod,
        int ReflectionMod,
        bool TargetNotSource,
        int BestReflect,
        int CandidateReflect);

    public static bool EvaluateReflectionScan(ReflectionScanContext ctx) =>
        ReflectionEligible(ctx.Damage, ctx.HasSource, ctx.ReflectionMod, ctx.Mod, ctx.TargetNotSource)
        && PickMaxReflection(ctx.CandidateReflect, ctx.BestReflect);

    public readonly record struct DoDamageMobjContext(
        bool MirrorTarget,
        bool MirrorInflictor,
        bool MirrorSource,
        bool VisualTarget,
        bool VisualInflictor,
        bool VisualSource,
        int RealDamage,
        bool NeedEvent);

    public static bool EvaluateDoDamageMobjMirror(DoDamageMobjContext ctx) =>
        MirrorActorSkips(ctx.MirrorTarget, ctx.MirrorInflictor, ctx.MirrorSource)
        || CoopVisualSkips(ctx.VisualTarget, ctx.VisualInflictor, ctx.VisualSource);

    public static bool RecordInvasionOnDamage(DoDamageMobjContext ctx) =>
        ctx.RealDamage > 0;

    public static bool ReactToDamageNonNegative(DoDamageMobjContext ctx) => ctx.RealDamage >= 0;

    public static bool EmitWorldDamagedEvent(DoDamageMobjContext ctx) =>
        ctx.RealDamage > 0 && ctx.NeedEvent;

    public readonly record struct PDamageMobjContext(
        bool MirrorTarget,
        bool MirrorInflictor,
        bool MirrorSource,
        bool VisualTarget,
        bool VisualInflictor,
        bool VisualSource,
        bool HasVirtual,
        int Retval);

    public static bool EvaluatePDamageMobjEarly(PDamageMobjContext ctx) =>
        EvaluateDoDamageMobjMirror(new DoDamageMobjContext(
            ctx.MirrorTarget, ctx.MirrorInflictor, ctx.MirrorSource,
            ctx.VisualTarget, ctx.VisualInflictor, ctx.VisualSource, 0, false));

    public static bool EvaluatePDamageMobjRecord(PDamageMobjContext ctx) =>
        ctx.Retval > 0 && RecordLagCompDamage(true, false);

    public readonly record struct CallOkaySwitchContext(bool HasVirtual, int VmReturn);

    public static bool EvaluateCallOkayToSwitchTarget(CallOkaySwitchContext ctx) =>
        !ctx.HasVirtual || CallReturnsTrue(ctx.VmReturn);

    public static bool PoisonMobjRequiresInflictor(bool inflictorNull) => !inflictorNull;

    public static bool PoisonDamagePlaysound(bool play) => play;

    public static int ClampDoDamageReturn(int real) => real < 0 ? 0 : real;
}

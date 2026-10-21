namespace HCDE.Playsim;

/// <summary>
/// <c>DLevelScript::GetActorProperty</c> and <c>CheckActorProperty</c> from <c>p_acs.cpp</c> (4408–4581).
/// </summary>
public static class VmAcsGetActorProperty
{
    public static bool ActorMissing(AActorGetRef? actor) => actor is null;

    public static int MissingActorResult() => 0;

    public static double DoubleToAcs(double value) => value * 65536.0;

    public static bool FlagBit(int flags, int mask) => (flags & mask) != 0;

    public static int TargetTidOrZero(AActorGetRef? actor) => actor?.TargetTid ?? 0;

    public static int TracerTidOrZero(AActorGetRef? actor) => actor?.TracerTid ?? 0;

    public static int MasterTidOrZero(AActorGetRef? actor) => actor?.MasterTid ?? 0;

    public enum GetKind
    {
        DefaultZero,
        Health,
        AcsDoubleScalar,
        IntScalar,
        RenderStyleLegacy,
        FlagBit,
        PlayerFloatVar,
        PlayerViewHeight,
        PlayerAttackZOffset,
        MasterTid,
        TargetTid,
        TracerTid,
        GlobalString,
        StencilColor,
    }

    public enum CheckKind
    {
        DefaultFalse,
        IntegerEquality,
        BooleanEquality,
        BehaviorStringEquality,
    }

    public readonly record struct GetPlan(GetKind Kind, bool RequiresPlayer, bool UsesBehaviorLookup);

    public readonly record struct CheckPlan(CheckKind Kind, bool RequiresPlayer, bool UsesBehaviorLookup);

    public static GetPlan PlanGet(VmAcsDoSetActorProperty.Aprop property) =>
        property switch
        {
            VmAcsDoSetActorProperty.Aprop.Health => new(GetKind.Health, false, false),
            VmAcsDoSetActorProperty.Aprop.Speed or VmAcsDoSetActorProperty.Aprop.Alpha
                or VmAcsDoSetActorProperty.Aprop.Gravity or VmAcsDoSetActorProperty.Aprop.DamageFactor
                or VmAcsDoSetActorProperty.Aprop.DamageMultiplier or VmAcsDoSetActorProperty.Aprop.ScaleX
                or VmAcsDoSetActorProperty.Aprop.ScaleY or VmAcsDoSetActorProperty.Aprop.Friction
                or VmAcsDoSetActorProperty.Aprop.MaxStepHeight or VmAcsDoSetActorProperty.Aprop.MaxDropOffHeight
                or VmAcsDoSetActorProperty.Aprop.MeleeRange
                => new(GetKind.AcsDoubleScalar, false, false),
            VmAcsDoSetActorProperty.Aprop.Damage or VmAcsDoSetActorProperty.Aprop.Score
                or VmAcsDoSetActorProperty.Aprop.Mass or VmAcsDoSetActorProperty.Aprop.Accuracy
                or VmAcsDoSetActorProperty.Aprop.Stamina or VmAcsDoSetActorProperty.Aprop.ReactionTime
                or VmAcsDoSetActorProperty.Aprop.FriendlySeeBlocks
                => new(GetKind.IntScalar, false, false),
            VmAcsDoSetActorProperty.Aprop.RenderStyle => new(GetKind.RenderStyleLegacy, false, false),
            VmAcsDoSetActorProperty.Aprop.Ambush or VmAcsDoSetActorProperty.Aprop.Invulnerable
                or VmAcsDoSetActorProperty.Aprop.Dropped or VmAcsDoSetActorProperty.Aprop.ChaseGoal
                or VmAcsDoSetActorProperty.Aprop.Frightened or VmAcsDoSetActorProperty.Aprop.Friendly
                or VmAcsDoSetActorProperty.Aprop.Notarget or VmAcsDoSetActorProperty.Aprop.Notrigger
                => new(GetKind.FlagBit, false, false),
            VmAcsDoSetActorProperty.Aprop.SpawnHealth => new(GetKind.IntScalar, false, false),
            VmAcsDoSetActorProperty.Aprop.JumpZ => new(GetKind.PlayerFloatVar, true, false),
            VmAcsDoSetActorProperty.Aprop.MasterTid => new(GetKind.MasterTid, false, false),
            VmAcsDoSetActorProperty.Aprop.TargetTid => new(GetKind.TargetTid, false, false),
            VmAcsDoSetActorProperty.Aprop.TracerTid => new(GetKind.TracerTid, false, false),
            VmAcsDoSetActorProperty.Aprop.ViewHeight => new(GetKind.PlayerViewHeight, true, false),
            VmAcsDoSetActorProperty.Aprop.AttackZOffset => new(GetKind.PlayerAttackZOffset, true, false),
            VmAcsDoSetActorProperty.Aprop.SeeSound or VmAcsDoSetActorProperty.Aprop.AttackSound
                or VmAcsDoSetActorProperty.Aprop.PainSound or VmAcsDoSetActorProperty.Aprop.DeathSound
                or VmAcsDoSetActorProperty.Aprop.ActiveSound or VmAcsDoSetActorProperty.Aprop.Species
                or VmAcsDoSetActorProperty.Aprop.NameTag or VmAcsDoSetActorProperty.Aprop.DamageType
                or VmAcsDoSetActorProperty.Aprop.SoundClass
                => new(GetKind.GlobalString, false, true),
            VmAcsDoSetActorProperty.Aprop.StencilColor => new(GetKind.StencilColor, false, false),
            _ => new(GetKind.DefaultZero, false, false),
        };

    public static CheckPlan PlanCheck(VmAcsDoSetActorProperty.Aprop property)
    {
        if (property is VmAcsDoSetActorProperty.Aprop.SeeSound or VmAcsDoSetActorProperty.Aprop.AttackSound
            or VmAcsDoSetActorProperty.Aprop.PainSound or VmAcsDoSetActorProperty.Aprop.DeathSound
            or VmAcsDoSetActorProperty.Aprop.ActiveSound or VmAcsDoSetActorProperty.Aprop.Species
            or VmAcsDoSetActorProperty.Aprop.NameTag or VmAcsDoSetActorProperty.Aprop.DamageType
            or VmAcsDoSetActorProperty.Aprop.SoundClass)
            return new(CheckKind.BehaviorStringEquality, false, true);

        if (property is VmAcsDoSetActorProperty.Aprop.Ambush or VmAcsDoSetActorProperty.Aprop.Invulnerable
            or VmAcsDoSetActorProperty.Aprop.Dropped or VmAcsDoSetActorProperty.Aprop.ChaseGoal
            or VmAcsDoSetActorProperty.Aprop.Frightened or VmAcsDoSetActorProperty.Aprop.Friendly
            or VmAcsDoSetActorProperty.Aprop.Notarget or VmAcsDoSetActorProperty.Aprop.Notrigger)
            return new(CheckKind.BooleanEquality, false, false);

        if (property is VmAcsDoSetActorProperty.Aprop.Health or VmAcsDoSetActorProperty.Aprop.Speed
            or VmAcsDoSetActorProperty.Aprop.Damage or VmAcsDoSetActorProperty.Aprop.DamageFactor
            or VmAcsDoSetActorProperty.Aprop.Alpha or VmAcsDoSetActorProperty.Aprop.RenderStyle
            or VmAcsDoSetActorProperty.Aprop.Gravity or VmAcsDoSetActorProperty.Aprop.SpawnHealth
            or VmAcsDoSetActorProperty.Aprop.JumpZ or VmAcsDoSetActorProperty.Aprop.Score
            or VmAcsDoSetActorProperty.Aprop.MasterTid or VmAcsDoSetActorProperty.Aprop.TargetTid
            or VmAcsDoSetActorProperty.Aprop.TracerTid or VmAcsDoSetActorProperty.Aprop.ScaleX
            or VmAcsDoSetActorProperty.Aprop.ScaleY or VmAcsDoSetActorProperty.Aprop.Mass
            or VmAcsDoSetActorProperty.Aprop.Accuracy or VmAcsDoSetActorProperty.Aprop.Stamina
            or VmAcsDoSetActorProperty.Aprop.ReactionTime or VmAcsDoSetActorProperty.Aprop.MeleeRange
            or VmAcsDoSetActorProperty.Aprop.ViewHeight or VmAcsDoSetActorProperty.Aprop.AttackZOffset
            or VmAcsDoSetActorProperty.Aprop.MaxStepHeight or VmAcsDoSetActorProperty.Aprop.MaxDropOffHeight
            or VmAcsDoSetActorProperty.Aprop.StencilColor or VmAcsDoSetActorProperty.Aprop.FriendlySeeBlocks)
            return new(CheckKind.IntegerEquality, property is VmAcsDoSetActorProperty.Aprop.JumpZ
                or VmAcsDoSetActorProperty.Aprop.ViewHeight or VmAcsDoSetActorProperty.Aprop.AttackZOffset, false);

        return new(CheckKind.DefaultFalse, false, false);
    }

    public static int LegacyRenderStyleIndex(int actorRenderStyle, ReadOnlySpan<int> legacyStyles, ReadOnlySpan<int> legacyIndices)
    {
        for (var style = 0; style < legacyStyles.Length && style < legacyIndices.Length; style++)
        {
            if (legacyStyles[style] == actorRenderStyle)
                return legacyIndices[style];
        }
        return 0;
    }

    public static int FlagMaskForGet(VmAcsDoSetActorProperty.Aprop property) =>
        VmAcsDoSetActorProperty.FlagMaskForProperty(property);

    public static bool PlayerOnlyZero(bool requiresPlayer, bool hasPlayer) => requiresPlayer && !hasPlayer;

    public static bool IntegerCheckMatches(int actual, int expected) => actual == expected;

    public static bool BooleanCheckMatches(bool actual, int expected) => actual == (expected != 0);

    public static bool BehaviorStringCheck(string actual, string expected) =>
        string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);

    public readonly record struct AActorGetRef(
        int Health,
        int Tid,
        bool HasPlayer,
        int Flags,
        int Flags2,
        int Flags3,
        int Flags4,
        int Flags5,
        int Flags6,
        int TargetTid,
        int TracerTid,
        int MasterTid);
}

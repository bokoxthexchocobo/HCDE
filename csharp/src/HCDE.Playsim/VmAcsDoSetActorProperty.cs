namespace HCDE.Playsim;

/// <summary><c>DLevelScript::DoSetActorProperty</c> from <c>p_acs.cpp</c> (4166–4406).</summary>
public static class VmAcsDoSetActorProperty
{
    /// <summary>Native <c>APROP_*</c> ids from <c>p_acs.cpp</c> (4074–4126).</summary>
    public enum Aprop
    {
        Health = 0,
        Speed = 1,
        Damage = 2,
        Alpha = 3,
        RenderStyle = 4,
        SeeSound = 5,
        AttackSound = 6,
        PainSound = 7,
        DeathSound = 8,
        ActiveSound = 9,
        Ambush = 10,
        Invulnerable = 11,
        JumpZ = 12,
        ChaseGoal = 13,
        Frightened = 14,
        Gravity = 15,
        Friendly = 16,
        SpawnHealth = 17,
        Dropped = 18,
        Notarget = 19,
        Species = 20,
        NameTag = 21,
        Score = 22,
        Notrigger = 23,
        DamageFactor = 24,
        MasterTid = 25,
        TargetTid = 26,
        TracerTid = 27,
        ScaleX = 29,
        ScaleY = 30,
        Mass = 32,
        Accuracy = 33,
        Stamina = 34,
        ReactionTime = 37,
        MeleeRange = 38,
        ViewHeight = 39,
        AttackZOffset = 40,
        StencilColor = 41,
        Friction = 42,
        DamageMultiplier = 43,
        MaxStepHeight = 44,
        MaxDropOffHeight = 45,
        DamageType = 46,
        SoundClass = 47,
        FriendlySeeBlocks = 48,
    }

    public const int MfAmbush = 0x200;
    public const int MfDropped = 0x8000;
    public const int MfFriendly = 0x40;
    public const int Mf2Invulnerable = 0x40;
    public const int Mf3Notarget = 0x80;
    public const int Mf4Frightened = 0x8000;
    public const int Mf5Chasegoal = 0x400;
    public const int Mf6Notrigger = 0x4000000;

    /// <summary><c>LegacyRenderStyleIndices</c> from <c>p_acs.cpp</c> (4129–4146).</summary>
    public static ReadOnlySpan<int> LegacyRenderStyleIndices =>
        [0, 1, 2, 3, 4, 5, 64, 65, 66, 67, 68, 69, 6, 7, -1];

    public static bool ActorIsNull(AActorRef? actor) => actor is null;

    public static bool ShouldSkipHealthOnDead(int health, bool playerDead) =>
        health <= 0 || playerDead;

    public static bool HealthShouldDie(int value) => value <= 0;

public static double AcsToDouble(int value) => value / 65536.0;

    public static float AcsToFloat(int value) => (float)AcsToDouble(value);

    public static int ToggleFlag(int flags, int mask, bool on) =>
        on ? flags | mask : flags & ~mask;

    public static int MonsterTotalBeforeFriendlyToggle(int totalMonsters, bool countsAsKill) =>
        countsAsKill ? totalMonsters - 1 : totalMonsters;

    public static int MonsterTotalAfterFriendlyToggle(int totalMonsters, bool countsAsKill) =>
        countsAsKill ? totalMonsters + 1 : totalMonsters;

    public static int ApplyFriendlyFlag(int flags, bool value) => ToggleFlag(flags, MfFriendly, value);

    public static bool RequiresPlayerPawn(Aprop property) =>
        property is Aprop.JumpZ or Aprop.SpawnHealth or Aprop.ViewHeight
            or Aprop.AttackZOffset or Aprop.SoundClass;

    public static bool RequiresPlayerWithSoundClass(Aprop property, bool hasPlayer) =>
        property == Aprop.SoundClass && hasPlayer;

    public static bool UsesBehaviorStringLookup(Aprop property) =>
        property is Aprop.SeeSound or Aprop.AttackSound or Aprop.PainSound
            or Aprop.DeathSound or Aprop.ActiveSound or Aprop.Species
            or Aprop.NameTag or Aprop.DamageType or Aprop.SoundClass;

    public static bool UsesAcsDouble(Aprop property) =>
        property is Aprop.Speed or Aprop.Alpha or Aprop.Gravity or Aprop.DamageFactor
            or Aprop.DamageMultiplier or Aprop.ScaleX or Aprop.ScaleY or Aprop.Friction
            or Aprop.MaxStepHeight or Aprop.MaxDropOffHeight;

    public static int FlagMaskForProperty(Aprop property) =>
        property switch
        {
            Aprop.Ambush => MfAmbush,
            Aprop.Dropped => MfDropped,
            Aprop.Invulnerable => Mf2Invulnerable,
            Aprop.Notarget => Mf3Notarget,
            Aprop.Notrigger => Mf6Notrigger,
            Aprop.ChaseGoal => Mf5Chasegoal,
            Aprop.Frightened => Mf4Frightened,
            _ => 0,
        };

    public static bool IsFlagProperty(Aprop property) => FlagMaskForProperty(property) != 0;

    public enum FlagField
    {
        Flags = 0,
        Flags2 = 1,
        Flags3 = 2,
        Flags4 = 3,
        Flags5 = 4,
        Flags6 = 5,
    }

    public readonly record struct PropertyDispatchDescriptor(
        bool NoOp,
        bool SkipWhenHealthDead,
        bool SyncPlayerHealth,
        bool DieOnNonPositiveHealth,
        bool MapLegacyRenderStyle,
        bool AdjustKillCountAroundFriendly,
        bool PlayerPawnOnly,
        bool RequiresPlayerForSoundClass,
        bool BehaviorStringLookup,
        bool MasterTidLookup,
        bool AssignIntScalar,
        bool AssignAcsDouble,
        bool StencilShade,
        int FlagMask,
        FlagField FlagField);

    /// <summary>Mirrors the <c>switch (property)</c> in <c>DoSetActorProperty</c> without hosted actor IO.</summary>
    public static PropertyDispatchDescriptor DescribePropertyDispatchNative(Aprop property)
    {
        return property switch
        {
            Aprop.Health => new(false, true, true, true, false, false, false, false, false, false, true, false, false, 0, FlagField.Flags),
            Aprop.Speed or Aprop.Alpha or Aprop.Gravity or Aprop.DamageFactor or Aprop.DamageMultiplier
                or Aprop.Friction or Aprop.MaxStepHeight or Aprop.MaxDropOffHeight or Aprop.MeleeRange
                => new(false, false, false, false, false, false, false, false, false, false, false, true, false, 0, FlagField.Flags),
            Aprop.Damage or Aprop.Score or Aprop.Mass or Aprop.Accuracy or Aprop.Stamina
                or Aprop.ReactionTime or Aprop.FriendlySeeBlocks
                => new(false, false, false, false, false, false, false, false, false, false, true, false, false, 0, FlagField.Flags),
            Aprop.RenderStyle => new(false, false, false, false, true, false, false, false, false, false, false, false, false, 0, FlagField.Flags),
            Aprop.Ambush => FlagDispatch(FlagField.Flags, MfAmbush),
            Aprop.Dropped => FlagDispatch(FlagField.Flags, MfDropped),
            Aprop.Invulnerable => FlagDispatch(FlagField.Flags2, Mf2Invulnerable),
            Aprop.Notarget => FlagDispatch(FlagField.Flags3, Mf3Notarget),
            Aprop.Notrigger => FlagDispatch(FlagField.Flags6, Mf6Notrigger),
            Aprop.ChaseGoal => FlagDispatch(FlagField.Flags5, Mf5Chasegoal),
            Aprop.Frightened => FlagDispatch(FlagField.Flags4, Mf4Frightened),
            Aprop.Friendly => new(false, false, false, false, false, true, false, false, false, false, false, false, false, MfFriendly, FlagField.Flags),
            Aprop.JumpZ or Aprop.SpawnHealth or Aprop.AttackZOffset
                => new(false, false, false, false, false, false, true, false, false, false, false, true, false, 0, FlagField.Flags),
            Aprop.ViewHeight => new(false, false, true, false, false, false, true, false, false, false, false, true, false, 0, FlagField.Flags),
            Aprop.SeeSound or Aprop.AttackSound or Aprop.PainSound or Aprop.DeathSound or Aprop.ActiveSound
                or Aprop.Species or Aprop.NameTag or Aprop.DamageType
                => new(false, false, false, false, false, false, false, false, true, false, false, false, false, 0, FlagField.Flags),
            Aprop.SoundClass => new(false, false, false, false, false, false, true, true, true, false, false, false, false, 0, FlagField.Flags),
            Aprop.MasterTid => new(false, false, false, false, false, false, false, false, false, true, false, false, false, 0, FlagField.Flags),
            Aprop.ScaleX or Aprop.ScaleY => new(false, false, false, false, false, false, false, false, false, false, false, true, false, 0, FlagField.Flags),
            Aprop.StencilColor => new(false, false, false, false, false, false, false, false, false, false, false, false, true, 0, FlagField.Flags),
            _ => new(true, false, false, false, false, false, false, false, false, false, false, false, false, 0, FlagField.Flags),
        };
    }

    private static PropertyDispatchDescriptor FlagDispatch(FlagField field, int mask) =>
        new(false, false, false, false, false, false, false, false, false, false, false, false, false, mask, field);

    public static bool TryMapLegacyRenderStyle(int legacyValue, ReadOnlySpan<int> table, out int renderStyleIndex)
    {
        for (var i = 0; i < table.Length; i++)
        {
            if (table[i] < 0)
                break;
            if (table[i] == legacyValue)
            {
                renderStyleIndex = i;
                return true;
            }
        }

        renderStyleIndex = -1;
        return false;
    }

    public readonly record struct AActorRef(int Health, bool PlayerDead, bool HasPlayer, bool CountsAsKill);
}

namespace HCDE.Playsim;

/// <summary><c>DLevelScript::DoSetActorProperty</c> from <c>p_acs.cpp</c> (4166–4406).</summary>
public static class VmAcsDoSetActorProperty
{
    public static bool ActorNull(AActorRef? actor) => actor is null;

    public static bool SkipHealthOnDead(int health, bool playerDead) =>
        health <= 0 || playerDead;

    public static bool ApplyHealth(int value, bool hasPlayer, out bool shouldDie)
    {
        shouldDie = value <= 0;
        return true;
    }

    public static double AcsToDouble(int value) => value / 65536.0;

    public static bool SetDamage(int value) => true;

    public static bool SetAlpha(double alpha) => alpha >= 0;

    public static bool TryMapLegacyRenderStyle(int value, ReadOnlySpan<int> table, out int index)
    {
        for (var i = 0; i < table.Length; i++)
        {
            if (table[i] == value)
            {
                index = i;
                return true;
            }
        }

        index = -1;
        return false;
    }

    public static bool SetRenderStyleIndex(int index) => index >= 0;

    public static bool UserVariableIndexValid(int index) => index >= 0;

    public static bool FriendlySeeBlocksProperty(int blocks) => blocks >= 0;

    public static bool UnknownPropertyNoOp() => true;

    public static bool PropertyHandled() => true;

    public readonly record struct AActorRef(int Health, bool PlayerDead, bool HasPlayer);

    public static bool SetHealthProperty(int value) => true;

    public static bool SetSpeedProperty(double speed) => speed >= 0;

    public static bool SetDamageProperty(int damage) => true;

    public static bool SetAlphaProperty(double alpha) => alpha >= 0 && alpha <= 1;

    public static bool SetRenderStyleProperty(int style) => style >= 0;

    public static bool SetGravityProperty(double gravity) => gravity >= 0;

    public static bool SetFrictionProperty(double friction) => friction >= 0;

    public static bool SetMassProperty(int mass) => mass >= 0;

    public static bool SetPainChanceProperty(int chance) => chance >= 0;

    public static bool SetSpeciesProperty(int species) => species >= 0;

    public static bool SetScoreProperty(int score) => true;

    public static bool SetDamageFactorProperty(double factor) => factor >= 0;

    public static bool SetStencilColorProperty(int color) => true;

    public static bool SetFillColorProperty(int color) => true;

    public static bool SetStencilOpacityProperty(int opacity) => opacity >= 0;

    public static bool SetSpawnFlagsProperty(int flags) => true;

    public static bool SetTranslationProperty(int translation) => translation >= 0;

    public static bool SetScaleProperty(float scale) => scale > 0;

    public static bool SetAngleProperty(double angle) => true;

    public static bool SetPitchProperty(double pitch) => true;

    public static bool SetRollProperty(double roll) => true;

    public static bool SetSeeSoundProperty(int sound) => sound >= 0;

    public static bool SetAttackSoundProperty(int sound) => sound >= 0;

    public static bool SetPainSoundProperty(int sound) => sound >= 0;

    public static bool SetDeathSoundProperty(int sound) => sound >= 0;

    public static bool SetActiveSoundProperty(int sound) => sound >= 0;

    public static bool SetRipSoundProperty(int sound) => sound >= 0;

    public static bool SetDroppedItemProperty(string item) => !string.IsNullOrEmpty(item);

    public static bool SetUserVariableProperty(int index, int value) =>
        index >= 0 && UserVariableIndexValid(index);

    public static bool SetSoundClassProperty(string cls) => !string.IsNullOrWhiteSpace(cls);

    public static bool SetFriendlySeeBlocksProperty(int blocks) => FriendlySeeBlocksProperty(blocks);

    public static bool DefaultCaseNoOp(int property) => property < 0;

    public const int MfAmbush = 0x200;
    public const int MfDropped = 0x8000;
    public const int MfFriendly = 0x40;
    public const int Mf2Invulnerable = 0x40;
    public const int Mf3Notarget = 0x80;
    public const int Mf4Frightened = 0x8000;
    public const int Mf5Chasegoal = 0x400;
    public const int Mf6Notrigger = 0x4000000;

    public static int ApplyFlagBit(int flags, int mask, bool value) =>
        value ? flags | mask : flags & ~mask;

    public static bool AmbushFromValue(int value) => value != 0;

    public static bool DroppedFromValue(int value) => value != 0;

    public static bool InvulnerableFromValue(int value) => value != 0;

    public static bool NotargetFromValue(int value) => value != 0;

    public static bool NotriggerFromValue(int value) => value != 0;

    public static bool ChaseGoalFromValue(int value) => value != 0;

    public static bool FrightenedFromValue(int value) => value != 0;

    public static bool FriendlyFromValue(int value) => value != 0;

    public static bool ShouldSkipJumpZ(bool isPlayerPawn) => !isPlayerPawn;

    public static bool ShouldSkipSpawnHealth(bool isPlayerPawn) => !isPlayerPawn;

    public static bool ShouldSkipViewHeight(bool isPlayerPawn) => !isPlayerPawn;

    public static bool ShouldSkipAttackZOffset(bool isPlayerPawn) => !isPlayerPawn;

    public static bool ShouldSkipSoundClass(bool isPlayerPawn, bool hasPlayer) =>
        !isPlayerPawn || !hasPlayer;

    public static float ScaleComponentFromAcs(int acsValue) =>
        (float)AcsToDouble(acsValue);

    public static bool FriendlyKillCountAdjust(bool countsAsKill) => countsAsKill;

    public static bool RenderStyleLoopContinues(int legacyValue, int tableValue) =>
        tableValue >= 0 && tableValue != legacyValue;

    public static bool RenderStyleLoopStops(int legacyValue, int tableValue) =>
        tableValue >= 0 && tableValue == legacyValue;

    public static bool HealthSyncsPlayer(bool hasPlayer) => hasPlayer;

    public static bool DieWhenHealthNonPositive(int value) => value <= 0;

    public static bool MasterTidRequiresLookup() => true;

    public static bool SpeciesUsesStringLookup() => true;

    public static bool NameTagUsesStringLookup() => true;

    public static bool DamageTypeUsesStringLookup() => true;

    public static bool SoundPropertiesUseLookup() => true;

    public static bool StencilUsesShade() => true;

    public static bool IsKnownProperty(int property) =>
        property switch
        {
            0 => true, // APROP_Health
            1 => true, // APROP_Speed
            2 => true, // APROP_Damage
            3 => true, // APROP_Alpha
            4 => true, // APROP_RenderStyle
            _ => property >= 0 && property < 64,
        };

    public static bool UsesAcsDouble(int property) =>
        property is 1 or 3 or 15 or 24 or 25 or 28 or 29 or 30 or 31 or 32 or 33 or 34 or 35;

    public static bool UsesDirectInt(int property) =>
        property is 0 or 2 or 5 or 6 or 7 or 8 or 9 or 10 or 11 or 12 or 13 or 14 or 16 or 17 or 18 or 19 or 20 or 21 or 22 or 23 or 26 or 27;
}

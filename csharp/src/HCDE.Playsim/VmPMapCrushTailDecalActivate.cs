namespace HCDE.Playsim;

/// <summary>
/// <c>P_DoCrunch</c> tail, shoot decals, deep splash, and <c>P_ActivateThingSpecial</c> from <c>p_map.cpp</c> (6679–7391).</summary>
public static class VmPMapCrushTailDecalActivate
{
    public const int Mf2Invulnerable = 1;
    public const int Mf2Dormant = 1 << 1;
    public const int Mf2Donttranslate = 1 << 2;
    public const int MfNoblood = 1;
    public const int Mf5Puffgetsowner = 1;
    public const int RfInvisible = 1;
    public const int ThingspecActivates = 1;
    public const int ThingspecDeactivate = 1 << 1;
    public const int ThingspecSwitch = 1 << 2;
    public const int ThingspecThingtargets = 1 << 3;
    public const int ThingspecTriggertargets = 1 << 4;
    public const int ThingspecClearspecial = 1 << 5;
    public const int ThingspecThingacts = 1 << 6;
    public const int ThingspecTriggeracts = 1 << 7;
    public const int LevelActownspecial = 1;
    public const int Level2Hexenhack = 1;

public static bool CrushDamageTick(int crushChange, int mapTime) =>
        crushChange > 0 && (mapTime & 3) == 0;

    public static bool SkipBloodWhenInvuln(int flags2) =>
        (flags2 & (Mf2Invulnerable | Mf2Dormant)) != 0;

    public static bool AllowBloodSpawn(int flags) => (flags & MfNoblood) == 0;

    public static int CrushBleedDamage(int newDam, int crushChange) =>
        newDam > 0 ? newDam : crushChange;

    public static bool ApplyBloodTranslation(int bloodTranslation, int moFlags2) =>
        bloodTranslation != -1 && (moFlags2 & Mf2Donttranslate) == 0;

    public static bool PuffGetsOwner(int flags5) => (flags5 & Mf5Puffgetsowner) != 0;

    public static bool HideBloodParticle(int bloodType) => bloodType > 1;

    public static bool DrawSplash2(int bloodType) => bloodType >= 1;

    public static bool PlayCrushPain(bool soundPlaying) => !soundPlaying;

    public static bool UseWeaponDecal(bool hasPlayer, bool hasReadyWeapon) =>
        hasPlayer && hasReadyWeapon;

public static bool DeepSplashHeightsec(bool crossedWater, bool hasHeightsec) =>
        crossedWater && hasHeightsec;

public static bool ThingTargetsTrigger(int activationType) =>
        (activationType & ThingspecThingtargets) != 0;

    public static bool TriggerTargetsThing(int activationType) =>
        (activationType & ThingspecTriggertargets) != 0;

    public static bool StateChangeWhenAlive(bool death, int activationType) =>
        !death && (activationType & (ThingspecActivates | ThingspecDeactivate | ThingspecSwitch)) != 0;

    public static int DefaultSwitchToActivate(int activationType)
    {
        if ((activationType & ThingspecSwitch) == 0)
            return activationType;
        if ((activationType & (ThingspecActivates | ThingspecDeactivate)) != 0)
            return activationType;
        return activationType | ThingspecActivates;
    }

    public static bool RunActivateBranch(int activationType) =>
        (activationType & ThingspecActivates) != 0;

    public static int ToggleSwitchAfterActivate(int activationType) =>
        (activationType & ~ThingspecActivates) | ThingspecDeactivate;

    public static bool RunDeactivateBranch(int activationType) =>
        (activationType & ThingspecDeactivate) != 0;

    public static int ToggleSwitchAfterDeactivate(int activationType) =>
        (activationType & ~ThingspecDeactivate) | ThingspecActivates;

    public readonly record struct ExecuteSpecialActorContext(
        bool Death,
        int LevelFlags,
        int ActivationType,
        bool HasSpecial);

    public static bool SpecialUsesThingAsActivator(ExecuteSpecialActorContext ctx) =>
        ctx.HasSpecial
        && ((ctx.Death && (ctx.LevelFlags & LevelActownspecial) != 0
             && (ctx.ActivationType & ThingspecTriggeracts) == 0)
            || (ctx.ActivationType & ThingspecThingacts) != 0);

    public static bool ClearSpecialOnDeath(ExecuteSpecialActorContext ctx, int levelFlags2) =>
        ctx.Death && (levelFlags2 & Level2Hexenhack) == 0;

    public static bool ClearSpecialOnSuccess(int activationType, bool executed) =>
        (activationType & ThingspecClearspecial) != 0 && executed;

    public readonly record struct ActivateBranchContext(int ActivationType, bool Death);

    public static bool ShouldEnterStateMachine(ActivateBranchContext ctx) =>
        StateChangeWhenAlive(ctx.Death, ctx.ActivationType);

    public static bool AfterActivateSetsDeactivate(int activationType, bool isSwitch) =>
        isSwitch && RunActivateBranch(activationType);

    public static bool AfterDeactivateSetsActivate(int activationType, bool isSwitch) =>
        isSwitch && RunDeactivateBranch(activationType);

    public static bool HasThingSpecial(int special) => special != 0;

public static bool MapTimeQuarter(int mapTime) => (mapTime & 3) == 0;
}

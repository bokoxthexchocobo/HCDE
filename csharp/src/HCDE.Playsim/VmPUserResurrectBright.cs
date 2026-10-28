namespace HCDE.Playsim;

/// <summary><c>player_t::Resurrect</c> and fullbright mode from <c>p_user.cpp</c> / <c>d_player.h</c> (1165–1268, 482–494).</summary>
public static class VmPUserResurrectBright
{
    public const int FbmodeNone = 0;
    public const int FbmodeDefault = 1;
    public const int FbmodeFullbright = 2;
    public const int FbmodeNightvision = 3;
    public const int FbmodeTorch = 4;

    public const int RfInvisible = 1;
    public const int Mf2Donttranslate = 1;

    public static bool ResurrectRejected(bool moNull, bool isPlayerChunk) => moNull || isPlayerChunk;

    public static bool ShouldRetranslatePlayer(int flags2) => (flags2 & Mf2Donttranslate) == 0;

    public static bool ShouldBringUpWeapon(object? readyWeapon) => readyWeapon != null;

    public static bool ShouldUnmorph(object? alternative) => alternative != null;

    public readonly record struct ResurrectPlan(
        bool MoNull,
        bool IsPlayerChunk,
        object? ReadyWeapon,
        object? Alternative,
        int Flags2);

    public static bool CanResurrect(ResurrectPlan plan) =>
        !ResurrectRejected(plan.MoNull, plan.IsPlayerChunk);

    public static bool ApplyTranslationAfterRevive(ResurrectPlan plan) =>
        CanResurrect(plan) && ShouldRetranslatePlayer(plan.Flags2);

    public static bool InvokeWeaponBringUp(ResurrectPlan plan) =>
        CanResurrect(plan) && ShouldBringUpWeapon(plan.ReadyWeapon);

    public static bool InvokeUnmorph(ResurrectPlan plan) =>
        CanResurrect(plan) && ShouldUnmorph(plan.Alternative);

    public static int ClearInvisibleRenderFlags(int renderFlags) => renderFlags & ~RfInvisible;

    public static int ResolveFullbrightMode(int storedMode, bool enhancedNightVision)
    {
        if (storedMode != FbmodeDefault) return storedMode;
        return enhancedNightVision ? FbmodeNightvision : FbmodeFullbright;
    }

    public readonly record struct FullbrightWrite(int Mode, bool Force);

    public static FullbrightWrite ApplyFullbrightMode(int mode, bool force) => new(mode, force);

    public static bool FullbrightDefaultUsesNightVision(int mode, bool enhancedNightVision) =>
        mode == FbmodeDefault && enhancedNightVision;

    public static bool FullbrightDefaultUsesClassic(int mode, bool enhancedNightVision) =>
        mode == FbmodeDefault && !enhancedNightVision;
}

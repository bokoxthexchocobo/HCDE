namespace HCDE.Playsim;

/// <summary>
/// <c>DLevelScript::RunScript</c> inventory/music/fade opcodes from <c>p_acs.cpp</c> (**9750–9899**).
/// Exclusive to chunk **947**; <c>AcsVm</c> dispatch is not matrix credit.
/// </summary>
public static class VmPAcsVmOpcode947Slice
{
    public const int CheckInventoryDirectPcWords = 1;
    public const int SetMusicStackPop = 3;
    public const int SetMusicDirectPcWords = 3;
    public const int FadeToStackPop = 5;
    public const int FadeRangeStackPop = 9;
    public const int SetAmmoCapacityStackPop = 2;
    public const int PlayMovieDisabledResult = -1;

    public static int CheckInventoryDirectPcAfterRead(int pc) =>
        pc + CheckInventoryDirectPcWords;

    public static int UseInventoryTypeIndex(int sp) => sp - 1;

    public static bool UseActorInventoryUsesWorld(int tid) => tid == 0;

    public static int AccumulateUseInventoryResult(int current, int used) => current + used;

    public static int UseActorInventoryResultSlot(int sp) => sp - 2;

    public static void ApplyUseActorInventoryStackPop(ref int sp) => sp--;

    public static int ResolveSigilPieces(object? activator, object? sigil, int sigilHealth)
    {
        if (activator is null || sigil is null)
            return 0;
        return sigilHealth;
    }

    public static bool TypeParentIsAmmo(int? parentClassId, int ammoClassId) =>
        parentClassId == ammoClassId;

    public static int ResolveAmmoCapacity(
        bool activatorPresent,
        bool parentIsAmmo,
        bool ownsAmmo,
        int ownedMax,
        int defaultMax)
    {
        if (!activatorPresent || !parentIsAmmo)
            return 0;
        return ownsAmmo ? ownedMax : defaultMax;
    }

    public static int SetAmmoCapacityAmountIndex(int sp) => sp - 1;

    public static int SetAmmoCapacityTypeIndex(int sp) => sp - 2;

    public static int InventoryAmountAfterGrant(int previousAmount) => previousAmount - previousAmount;

    public static void ApplySetAmmoCapacityStackPop(ref int sp) => sp -= SetAmmoCapacityStackPop;

    public static int SetMusicOrderIndex(int sp) => sp - 3;

    public static int SetMusicLoopIndex(int sp) => sp - 2;

    public static void ApplySetMusicStackPop(ref int sp) => sp -= SetMusicStackPop;

    public static int SetMusicDirectPcAfterRead(int pc) => pc + SetMusicDirectPcWords;

    public static bool ShouldApplyLocalSetMusic(bool isConsolePlayer, int opcode) =>
        isConsolePlayer
        && (opcode == (int)HCDE.MapLoader.AcsPcode.LocalSetMusic
            || opcode == (int)HCDE.MapLoader.AcsPcode.LocalSetMusicDirect);

    public static int LocalSetMusicDirectPcAfterRead(int pc) => pc + SetMusicDirectPcWords;

    public static int FadeToColorIndex(int sp) => sp - 5;

    public static int FadeToAmountIndex(int sp) => sp - 1;

    public static void ApplyFadeToStackPop(ref int sp) => sp -= FadeToStackPop;

    public static void ApplyFadeRangeStackPop(ref int sp) => sp -= FadeRangeStackPop;

    public static bool CancelFadeForAllPlayers(object? activator) => activator is null;

    public static bool CancelFadeForFaderOwner(object? activator, object? faderOwner) =>
        activator is not null && ReferenceEquals(activator, faderOwner);

    public static bool ShouldCancelFader(bool forAllPlayers, bool forOwner) => forAllPlayers || forOwner;

    public static int ResolvePlayMovieStackValue(int previousTop) =>
        previousTop < 0 ? previousTop : PlayMovieDisabledResult;

    public static bool SetsMusic(int opcode) =>
        opcode == (int)HCDE.MapLoader.AcsPcode.SetMusic
        || opcode == (int)HCDE.MapLoader.AcsPcode.SetMusicDirect
        || opcode == (int)HCDE.MapLoader.AcsPcode.LocalSetMusic
        || opcode == (int)HCDE.MapLoader.AcsPcode.LocalSetMusicDirect;
}

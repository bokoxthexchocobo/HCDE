namespace HCDE.Playsim;

/// <summary>
/// <c>DLevelScript::RunScript</c> gravity/spawn/inventory opcodes from <c>p_acs.cpp</c> (**9600–9749**).
/// Exclusive to chunk **946**; <c>AcsVm</c> dispatch is not matrix credit.
/// </summary>
public static class VmPAcsVmOpcode946Slice
{
    public const int SpawnArgCount = 6;
    public const int SpawnStackPop = 5;
    public const int SpawnDirectPcWords = 6;
    public const int SpawnSpotStackPop = 3;
    public const int SpawnSpotDirectPcWords = 4;
    public const int SpawnSpotFacingStackPop = 2;
    public const int InventoryDirectPcWords = 2;
    public const int GiveActorInventoryStackPop = 3;
    public const int TakeActorInventoryStackPop = 3;
    public const int CheckActorInventoryStackPop = 1;

    public static double LevelScalarFromFixed(int fixedValue) => fixedValue / 65536.0;

    public static bool SetsLevelGravity(int opcode) =>
        opcode == (int)HCDE.MapLoader.AcsPcode.SetGravity
        || opcode == (int)HCDE.MapLoader.AcsPcode.SetGravityDirect;

    public static int AdvanceProgramCounter(int pc, int wordCount) => pc + wordCount;

    public static int PopAfterUnarySet(ref int sp) => --sp;

    public static int SpawnResultSlot(int sp) => sp - SpawnArgCount;

    public static int SpawnSpotResultSlot(int sp) => sp - 4;

    public static int SpawnSpotFacingResultSlot(int sp) => sp - 3;

    public static void ApplySpawnStackPop(ref int sp) => sp -= SpawnStackPop;

    public static void ApplySpawnSpotStackPop(ref int sp) => sp -= SpawnSpotStackPop;

    public static void ApplySpawnSpotFacingStackPop(ref int sp) => sp -= SpawnSpotFacingStackPop;

    public static bool ClearActorInventoryUsesWorldTarget(int tid) => tid == 0;

    public static bool ActorInventoryUsesActivatorTid(int tid) => tid == 0;

    public static int GiveInventoryAmountIndex(int sp) => sp - 1;

    public static int GiveInventoryTypeIndex(int sp) => sp - 2;

    public static void ApplyGiveInventoryStackPop(ref int sp) => sp -= 2;

    public static int TakeInventoryAmountIndex(int sp) => sp - 1;

    public static int TakeInventoryTypeIndex(int sp) => sp - 2;

    public static void ApplyTakeInventoryStackPop(ref int sp) => sp -= 2;

    public static int GiveActorInventoryAmountIndex(int sp) => sp - 1;

    public static int GiveActorInventoryTypeIndex(int sp) => sp - 2;

    public static int GiveActorInventoryTidIndex(int sp) => sp - 3;

    public static int TakeActorInventoryAmountIndex(int sp) => sp - 1;

    public static int TakeActorInventoryTypeIndex(int sp) => sp - 2;

    public static int TakeActorInventoryTidIndex(int sp) => sp - 3;

    public static void ApplyGiveActorInventoryStackPop(ref int sp) => sp -= GiveActorInventoryStackPop;

    public static void ApplyTakeActorInventoryStackPop(ref int sp) => sp -= TakeActorInventoryStackPop;

    public static int CheckInventoryReplacesTop(int sp) => sp - 1;

    public static int CheckActorInventoryTidIndex(int sp) => sp - 2;

    public static int CheckActorInventoryTypeIndex(int sp) => sp - 1;

    public static void ApplyCheckActorInventoryStackPop(ref int sp) => sp -= CheckActorInventoryStackPop;

    public static bool TriggersAirControlChanged(int opcode) =>
        opcode == (int)HCDE.MapLoader.AcsPcode.SetAirControl
        || opcode == (int)HCDE.MapLoader.AcsPcode.SetAirControlDirect;

    public static int SpawnArgFromTop(int sp, int argIndexFromOne) =>
        sp - argIndexFromOne;

    public static int SpawnDirectPcAfterRead(int pc) =>
        AdvanceProgramCounter(pc, SpawnDirectPcWords);

    public static int SpawnSpotDirectPcAfterRead(int pc) =>
        AdvanceProgramCounter(pc, SpawnSpotDirectPcWords);

    public static int InventoryDirectPcAfterRead(int pc) =>
        AdvanceProgramCounter(pc, InventoryDirectPcWords);

    public const bool NativeDoSpawnForceArgument = false;

    public static bool SpawnUsesNonForcedPlacement() => !NativeDoSpawnForceArgument;
}

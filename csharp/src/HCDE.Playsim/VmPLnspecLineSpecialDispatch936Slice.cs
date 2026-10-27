namespace HCDE.Playsim;

/// <summary>
/// <c>LineSpecials[]</c> tail (opcodes **208–284**) and line-special registry head from <c>p_lnspec.cpp</c> (**3755–3904**); chunk **936**.
/// </summary>
public static class VmPLnspecLineSpecialDispatch936Slice
{
    public const int DispatchBaseOpcode = 208;
    public const int DispatchEndOpcode = 284;

    public static readonly string[] DispatchOpcodes208Through284 =
    [
        "LS_TranslucentLine",
        "LS_NOP",
        "LS_NOP",
        "LS_NOP",
        "LS_Sector_SetColor",
        "LS_Sector_SetFade",
        "LS_Sector_SetDamage",
        "LS_Teleport_Line",
        "LS_Sector_SetGravity",
        "LS_Stairs_BuildUpDoom",
        "LS_Sector_SetWind",
        "LS_Sector_SetFriction",
        "LS_Sector_SetCurrent",
        "LS_Scroll_Texture_Both",
        "LS_NOP",
        "LS_Scroll_Floor",
        "LS_Scroll_Ceiling",
        "LS_NOP",
        "LS_ACS_ExecuteAlways",
        "LS_PointPush_SetForce",
        "LS_Plat_RaiseAndStayTx0",
        "LS_Thing_SetGoal",
        "LS_Plat_UpByValueStayTx",
        "LS_Plat_ToggleCeiling",
        "LS_Light_StrobeDoom",
        "LS_Light_MinNeighbor",
        "LS_Light_MaxNeighbor",
        "LS_Floor_TransferTrigger",
        "LS_Floor_TransferNumeric",
        "LS_ChangeCamera",
        "LS_Floor_RaiseToLowestCeiling",
        "LS_Floor_RaiseByValueTxTy",
        "LS_Floor_RaiseByTexture",
        "LS_Floor_LowerToLowestTxTy",
        "LS_Floor_LowerToHighest",
        "LS_Exit_Normal",
        "LS_Exit_Secret",
        "LS_Elevator_RaiseToNearest",
        "LS_Elevator_MoveToFloor",
        "LS_Elevator_LowerToNearest",
        "LS_HealThing",
        "LS_Door_CloseWaitOpen",
        "LS_Floor_Donut",
        "LS_FloorAndCeiling_LowerRaise",
        "LS_Ceiling_RaiseToNearest",
        "LS_Ceiling_LowerToLowest",
        "LS_Ceiling_LowerToFloor",
        "LS_Ceiling_CrushRaiseAndStaySilA",
        "LS_Floor_LowerToHighestEE",
        "LS_Floor_RaiseToLowest",
        "LS_Floor_LowerToLowestCeiling",
        "LS_Floor_RaiseToCeiling",
        "LS_Floor_ToCeilingInstant",
        "LS_Floor_LowerByTexture",
        "LS_Ceiling_RaiseToHighest",
        "LS_Ceiling_ToHighestInstant",
        "LS_Ceiling_LowerToNearest",
        "LS_Ceiling_RaiseToLowest",
        "LS_Ceiling_RaiseToHighestFloor",
        "LS_Ceiling_ToFloorInstant",
        "LS_Ceiling_RaiseByTexture",
        "LS_Ceiling_LowerByTexture",
        "LS_Stairs_BuildDownDoom",
        "LS_Stairs_BuildUpDoomSync",
        "LS_Stairs_BuildDownDoomSync",
        "LS_Stairs_BuildUpDoomCrush",
        "LS_Door_AnimatedClose",
        "LS_Floor_Stop",
        "LS_Ceiling_Stop",
        "LS_Sector_SetFloorGlow",
        "LS_Sector_SetCeilingGlow",
        "LS_Floor_MoveToValueAndCrush",
        "LS_Ceiling_MoveToValueAndCrush",
        "LS_Line_SetAutomapFlags",
        "LS_Line_SetAutomapStyle",
        "LS_Polyobj_StopSound",
        "LS_Generic_CrusherDist",
    ];

    public static int TailIndex(int opcode) => opcode - DispatchBaseOpcode;

    public static bool TailOpcodeInRange(int opcode) =>
        opcode >= DispatchBaseOpcode && opcode <= DispatchEndOpcode;

    public static string? TailDispatchName(int opcode) =>
        TailOpcodeInRange(opcode) ? DispatchOpcodes208Through284[TailIndex(opcode)] : null;

    public static bool TailDispatchIsNop(int opcode) => TailDispatchName(opcode) == "LS_NOP";

    public static bool TailDispatchIsSectorEnv(int opcode) => opcode is >= 212 and <= 220;

    public static bool TailDispatchIsElevator(int opcode) => opcode is >= 245 and <= 247;

    public static bool TailDispatchIsExit(int opcode) => opcode is 243 or 244;

    public static int TailDispatchEntryCount() => DispatchOpcodes208Through284.Length;

    public static int ScanMaxSpecialNumber(ReadOnlySpan<int> specialNumbers)
    {
        var max = 0;
        foreach (var number in specialNumbers)
        {
            if (number > max)
                max = number;
        }
        return max;
    }

    public static int RegistrySlotCount(int maxSpecialNumber) => maxSpecialNumber + 1;

    public static int MaxLineSpecialFromRegistrySize(int registrySize) => registrySize - 1;

    public static bool LineSpecialInfoInRange(int special, int registrySize) =>
        (uint)special < (uint)registrySize;

    public static bool CanAssignRegistrySlot(int number, int registryLength) =>
        number >= 0 && number < registryLength;
}

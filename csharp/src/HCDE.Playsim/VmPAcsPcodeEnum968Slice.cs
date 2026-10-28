namespace HCDE.Playsim;

/// <summary>
/// Baseline ACS P-code opcode constants from the engine enum in <c>p_acs.cpp</c> (**1–150** / opcodes **0–69**).
/// Exclusive to chunk **968**.
/// </summary>
public static class VmPAcsPcodeEnum968Slice
{
    public const int PcdNop = 0;
    public const int PcdTerminate = 1;
    public const int PcdSuspend = 2;
    public const int PcdPushnumber = 3;
    public const int PcdLspec1 = 4;
    public const int PcdLspec2 = 5;
    public const int PcdLspec3 = 6;
    public const int PcdLspec4 = 7;
    public const int PcdLspec5 = 8;
    public const int PcdLspec1Direct = 9;
    public const int PcdLspec2Direct = 10;
    public const int PcdLspec3Direct = 11;
    public const int PcdLspec4Direct = 12;
    public const int PcdLspec5Direct = 13;
    public const int PcdAdd = 14;
    public const int PcdSubtract = 15;
    public const int PcdMultiply = 16;
    public const int PcdDivide = 17;
    public const int PcdModulus = 18;
    public const int PcdEq = 19;
    public const int PcdNe = 20;
    public const int PcdLt = 21;
    public const int PcdGt = 22;
    public const int PcdLe = 23;
    public const int PcdGe = 24;
    public const int PcdAssignscriptvar = 25;
    public const int PcdAssignmapvar = 26;
    public const int PcdAssignworldvar = 27;
    public const int PcdPushscriptvar = 28;
    public const int PcdPushmapvar = 29;
    public const int PcdPushworldvar = 30;
    public const int PcdAddscriptvar = 31;
    public const int PcdAddmapvar = 32;
    public const int PcdAddworldvar = 33;
    public const int PcdSubscriptvar = 34;
    public const int PcdSubmapvar = 35;
    public const int PcdSubworldvar = 36;
    public const int PcdMulscriptvar = 37;
    public const int PcdMulmapvar = 38;
    public const int PcdMulworldvar = 39;
    public const int PcdDivscriptvar = 40;
    public const int PcdDivmapvar = 41;
    public const int PcdDivworldvar = 42;
    public const int PcdModscriptvar = 43;
    public const int PcdModmapvar = 44;
    public const int PcdModworldvar = 45;
    public const int PcdIncscriptvar = 46;
    public const int PcdIncmapvar = 47;
    public const int PcdIncworldvar = 48;
    public const int PcdDecscriptvar = 49;
    public const int PcdDecmapvar = 50;
    public const int PcdDecworldvar = 51;
    public const int PcdGoto = 52;
    public const int PcdIfgoto = 53;
    public const int PcdDrop = 54;
    public const int PcdDelay = 55;
    public const int PcdDelaydirect = 56;
    public const int PcdRandom = 57;
    public const int PcdRandomdirect = 58;
    public const int PcdThingcount = 59;
    public const int PcdThingcountdirect = 60;
    public const int PcdTagwait = 61;
    public const int PcdTagwaitdirect = 62;
    public const int PcdPolywait = 63;
    public const int PcdPolywaitdirect = 64;
    public const int PcdChangefloor = 65;
    public const int PcdChangefloordirect = 66;
    public const int PcdChangeceiling = 67;
    public const int PcdChangeceilingdirect = 68;
    public const int PcdRestart = 69;

    public const int BaselineOpcodeMax = PcdRestart;

    public static bool IsBaselineOpcode(int opcode) =>
        opcode >= PcdNop && opcode <= BaselineOpcodeMax;

    public static bool IsLspecStackOpcode(int opcode) =>
        opcode >= PcdLspec1 && opcode <= PcdLspec5;

    public static bool IsLspecDirectOpcode(int opcode) =>
        opcode >= PcdLspec1Direct && opcode <= PcdLspec5Direct;

    public static int LspecArgCount(int opcode)
    {
        if (IsLspecStackOpcode(opcode))
            return opcode - PcdLspec1 + 1;
        if (IsLspecDirectOpcode(opcode))
            return opcode - PcdLspec1Direct + 1;
        return 0;
    }

    public static bool IsCompareOpcode(int opcode) =>
        opcode >= PcdEq && opcode <= PcdGe;

    public static bool IsBinaryArithmeticOpcode(int opcode) =>
        opcode >= PcdAdd && opcode <= PcdModulus;

    public static string BaselineOpcodeLabel(int opcode) => opcode switch
    {
        PcdNop => "PCD_NOP",
        PcdTerminate => "PCD_TERMINATE",
        PcdSuspend => "PCD_SUSPEND",
        PcdPushnumber => "PCD_PUSHNUMBER",
        PcdRestart => "PCD_RESTART",
        PcdGoto => "PCD_GOTO",
        PcdIfgoto => "PCD_IFGOTO",
        _ when IsLspecStackOpcode(opcode) => $"PCD_LSPEC{LspecArgCount(opcode)}",
        _ when IsLspecDirectOpcode(opcode) => $"PCD_LSPEC{LspecArgCount(opcode)}DIRECT",
        _ => $"PCD_{opcode}",
    };
}

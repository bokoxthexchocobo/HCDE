namespace HCDE.Playsim;

/// <summary>
/// <c>DLevelScript::RunScript</c> <c>STRCPYTO*CHRANGE</c> tail, console commands, epilogue, and
/// <c>P_GetScriptGoing</c> head from <c>p_acs.cpp</c> (**10650–10799**). Exclusive to chunk **949**.
/// </summary>
public static class VmPAcsRunscriptStrcpyEpilogue949Slice
{
    public const int StrCpyRangeStackPop = 5;
    public const int ConsoleCommandStackPop = 3;
    public const int ConsoleCommandDirectPcWords = 3;
    public const int NumMapVars = 128;

    public static int StrCpyDestinationIndex(int baseIndex, int offset) => baseIndex + offset;

    public static bool StrCpyRangeArgsInvalid(int destIndex, int sourceOffset) =>
        destIndex < 0 || sourceOffset < 0;

    public static void ApplyStrCpyRangeStackPop(ref int sp) => sp -= StrCpyRangeStackPop;

    public static bool StrCpyCompletesWhenLookupEmpty(string? lookup) =>
        string.IsNullOrEmpty(lookup);

    public static int AdvanceStringOffset(string text, int offset)
    {
        if (offset <= 0)
            return 0;
        var index = Math.Min(offset, text.Length);
        return index;
    }

    public static bool StrCpySuccessWhenOnlyTerminatorRemains(char nextChar) => nextChar == '\0';

    public static bool MapCharRangeArrayWritable(int arrayId, bool mapVarPresent) =>
        arrayId > 0 && arrayId < NumMapVars && mapVarPresent;

    public static bool IsStrCpyToMapCharRange(int opcode) =>
        opcode == (int)HCDE.MapLoader.AcsPcode.StrCpyToMapCharRange;

    public static bool IsStrCpyToWorldCharRange(int opcode) =>
        opcode == (int)HCDE.MapLoader.AcsPcode.StrCpyToWorldCharRange;

    public static bool IsStrCpyToGlobalCharRange(int opcode) =>
        opcode == (int)HCDE.MapLoader.AcsPcode.StrCpyToGlobalCharRange;

    public static bool IsStrCpyToScriptCharRange(int opcode) =>
        opcode == (int)HCDE.MapLoader.AcsPcode.StrCpyToScriptCharRange;

    public static int ConsoleCommandStringIdIndex(int sp) => sp - 3;

    public static void ApplyConsoleCommandStackPop(ref int sp) => sp -= ConsoleCommandStackPop;

    public static int ConsoleCommandDirectPcAfterRead(int pc) => pc + ConsoleCommandDirectPcWords;

    public static bool ShouldRestoreSavedBehavior(VmPAcsIsClientSideScript.ScriptState state) =>
        state == VmPAcsIsClientSideScript.ScriptState.DivideBy0
        || state == VmPAcsIsClientSideScript.ScriptState.ModulusBy0;

    public static VmPAcsIsClientSideScript.ScriptState StateAfterDivideByZero() =>
        VmPAcsIsClientSideScript.ScriptState.PleaseRemove;

    public static VmPAcsIsClientSideScript.ScriptState StateAfterModulusByZero() =>
        VmPAcsIsClientSideScript.ScriptState.PleaseRemove;

    public static bool ShouldUnlinkRunningScript(
        VmPAcsIsClientSideScript.ScriptState state,
        bool runningEntryMatches) =>
        state == VmPAcsIsClientSideScript.ScriptState.PleaseRemove && runningEntryMatches;

    public static bool RunScriptEndsWithEmptyStack(int sp) => sp == 0;

    public static bool ShouldIgnoreClientSideScriptStart(bool clientSide, bool ignoreActivator) =>
        clientSide && ignoreActivator;

    public static bool ResumeSuspendedRunningScript(VmPAcsIsClientSideScript.ScriptState state) =>
        state == VmPAcsIsClientSideScript.ScriptState.Suspended;

    public static bool SkipNewScriptWhenAlreadyRunning(bool alwaysFlag, bool hasRunningEntry) =>
        !alwaysFlag && hasRunningEntry;
}

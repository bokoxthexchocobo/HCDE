namespace HCDE.Playsim;

/// <summary>
/// <c>P_GetScriptGoing</c> tail, <c>DLevelScript</c> constructor, <c>SetScriptState</c>,
/// <c>FLevelLocals::DoDeferedScripts</c>, and <c>addDefered</c> head from <c>p_acs.cpp</c> (**10800–10949**).
/// Exclusive to chunk **950**; live ACS script hosting is not matrix credit.
/// </summary>
public static class VmPAcsDLevelScriptCtor950Slice
{
    public const int DeferredScriptArgSlots = 3;
    public const int DeferExecute = 0;
    public const int DeferExecuteAlways = 1;
    public const int DeferSuspend = 2;
    public const int DeferTerminate = 3;
    public const int MissingPlayerNum = -1;

    public static VmPAcsIsClientSideScript.ScriptState ResumeSuspendedToRunning() =>
        VmPAcsIsClientSideScript.ScriptState.Running;

    public static bool AlreadyRunningNonSuspended(bool hasRunning, VmPAcsIsClientSideScript.ScriptState state) =>
        hasRunning && state != VmPAcsIsClientSideScript.ScriptState.Suspended;

    public static bool ShouldCreateClientSideThinker(bool clientSide, object? existingThinker) =>
        clientSide && existingThinker is null;

    public static bool ShouldCreateServerAcsThinker(bool clientSide, object? existingThinker) =>
        !clientSide && existingThinker is null;

    public static int LocalVarInitCount(int varCount, int argCount) =>
        Math.Min(argCount, varCount);

    public static bool LocalVarCapacityValid(int varCount, int argCount) => varCount >= argCount;

    public static bool ActivationBackSide(int flags) =>
        (flags & VmPLnspecArgMacros.AcsBacksideFlag(true)) != 0;

    public static bool ShouldTrackInRunningScripts(int flags) =>
        (flags & VmPLnspecArgMacros.AcsAlwaysFlag) == 0;

    public static bool ShouldPutLastForHexenHack(bool clientSide, bool levelHexenHack) =>
        !clientSide && levelHexenHack;

    public static bool TrySetScriptStateOnController(
        object? controller,
        bool hasRunningEntry,
        VmPAcsIsClientSideScript.ScriptState newState) =>
        controller is not null && hasRunningEntry;

    public static VmPAcsIsClientSideScript.ScriptState StateForDeferSuspend() =>
        VmPAcsIsClientSideScript.ScriptState.Suspended;

    public static VmPAcsIsClientSideScript.ScriptState StateForDeferTerminate() =>
        VmPAcsIsClientSideScript.ScriptState.PleaseRemove;

    public static int FlagsForDeferredExecute(int deferType) =>
        deferType == DeferExecuteAlways ? VmPLnspecArgMacros.AcsAlwaysFlag : 0;

    public static bool IsDeferredExecuteType(int deferType) =>
        deferType == DeferExecute || deferType == DeferExecuteAlways;

    public static int CopyDeferredArgs(Span<int> destination, ReadOnlySpan<int> source)
    {
        var copied = Math.Min(destination.Length, source.Length);
        for (var i = 0; i < copied; i++)
            destination[i] = source[i];
        for (var j = copied; j < destination.Length; j++)
            destination[j] = 0;
        return copied;
    }

    public static int ResolveDeferredPlayerNum(object? activator, object? player, int levelPlayerNum) =>
        activator is not null && player is not null ? levelPlayerNum : MissingPlayerNum;

    public static bool DeferredListHasEntries(int count) => count > 0;
}

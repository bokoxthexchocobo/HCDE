namespace HCDE.Playsim;

/// <summary>
/// <c>addDefered</c> tail, <c>P_StartScript</c>, suspend/terminate routing, deferred serialize,
/// <c>scriptstat</c>, and <c>DACSThinker::DumpScriptStatus</c> head from <c>p_acs.cpp</c> (**10950–11099**).
/// Exclusive to chunk **951**; live ACS hosting is not matrix credit.
/// </summary>
public static class VmPAcsStartScript951Slice
{
    public const int MapNameCompareLength = 8;
    public const int AcsNetFlag = 8;
    public const int ScriptfNet = 0x0001;
    public const int DeferredSerializeArgSlots = 3;
    public const int DeferredSerializerFieldCount = 4;

    public static int MissingDeferredPlayerNum => VmPAcsDLevelScriptCtor950Slice.MissingPlayerNum;

    public static bool MapNamesMatchForScript(ReadOnlySpan<char> levelMap, ReadOnlySpan<char> targetMap)
    {
        if (targetMap.IsEmpty)
            return true;
        var limit = Math.Min(MapNameCompareLength, Math.Min(levelMap.Length, targetMap.Length));
        for (var i = 0; i < limit; i++)
        {
            var a = char.ToUpperInvariant(levelMap[i]);
            var b = char.ToUpperInvariant(targetMap[i]);
            if (a != b)
                return false;
        }
        return true;
    }

    public static bool ShouldDeferStartOnOtherMap(ReadOnlySpan<char> levelMap, ReadOnlySpan<char> targetMap) =>
        !targetMap.IsEmpty && !MapNamesMatchForScript(levelMap, targetMap);

    public static int DeferredExecuteTypeForStartFlags(int flags) =>
        (flags & VmPLnspecArgMacros.AcsAlwaysFlag) != 0
            ? VmPAcsDLevelScriptCtor950Slice.DeferExecuteAlways
            : VmPAcsDLevelScriptCtor950Slice.DeferExecute;

    public static bool AllowNonNetScriptNetRequest(
        int scriptFlags,
        bool svAllowAllScripts,
        bool allowCompatSettingsScript,
        bool allowSinglePlayerScripts,
        bool netGame,
        bool localConsoleRequester) =>
        (scriptFlags & ScriptfNet) != 0
        || svAllowAllScripts
        || allowCompatSettingsScript
        || (!netGame && allowSinglePlayerScripts)
        || localConsoleRequester;

    public static bool DenyNonNetScriptRequest(
        int flags,
        int scriptFlags,
        bool svAllowAllScripts,
        bool allowCompatSettingsScript,
        bool allowSinglePlayerScripts,
        bool netGame,
        bool localConsoleRequester) =>
        (flags & AcsNetFlag) != 0 && !AllowNonNetScriptNetRequest(
            scriptFlags,
            svAllowAllScripts,
            allowCompatSettingsScript,
            allowSinglePlayerScripts,
            netGame,
            localConsoleRequester);

    public static bool ShouldLogUnauthorizedPukeAttempt(
        bool localServiceAuthority,
        bool hasActivatorPlayer,
        bool clientSideScript) =>
        localServiceAuthority && hasActivatorPlayer && !clientSideScript;

    public static bool StartScriptReturnsRunResult(int flags) =>
        (flags & VmPLnspecArgMacros.AcsWantResultFlag) != 0;

    public static int StartScriptResultWhenRunning(bool wantRunResult, int runScriptResult, bool scriptStarted) =>
        wantRunResult ? runScriptResult : scriptStarted ? 1 : 0;

    public static bool ShouldPrintUnknownScript(int flags, bool activatorIsConsolePlayer) =>
        (flags & AcsNetFlag) == 0 || activatorIsConsolePlayer;

    public static bool SuspendOrTerminateOnOtherMap(ReadOnlySpan<char> levelMap, ReadOnlySpan<char> targetMap) =>
        !MapNamesMatchForScript(levelMap, targetMap);

    public static int DeferTypeForSuspend() => VmPAcsDLevelScriptCtor950Slice.DeferSuspend;

    public static int DeferTypeForTerminate() => VmPAcsDLevelScriptCtor950Slice.DeferTerminate;

    public static VmPAcsIsClientSideScript.ScriptState LocalStateForDeferredSuspend() =>
        VmPAcsDLevelScriptCtor950Slice.StateForDeferSuspend();

    public static VmPAcsIsClientSideScript.ScriptState LocalStateForDeferredTerminate() =>
        VmPAcsDLevelScriptCtor950Slice.StateForDeferTerminate();

    public static int PadDeferredArgsToSlotCount(int copiedSlots, int slotCount) =>
        copiedSlots < slotCount ? slotCount - copiedSlots : 0;

    public static bool LevelHasNoAcsThinker(object? acsThinker) => acsThinker is null;

    public static bool DumpScriptStatusHasNext(object? script) => script is not null;

    public static string ScriptStateName(VmPAcsIsClientSideScript.ScriptState state) =>
        state switch
        {
            VmPAcsIsClientSideScript.ScriptState.Running => "Running",
            VmPAcsIsClientSideScript.ScriptState.Suspended => "Suspended",
            VmPAcsIsClientSideScript.ScriptState.Delayed => "Delayed",
            VmPAcsIsClientSideScript.ScriptState.TagWait => "TagWait",
            VmPAcsIsClientSideScript.ScriptState.PolyWait => "PolyWait",
            VmPAcsIsClientSideScript.ScriptState.ScriptWaitPre => "ScriptWaitPre",
            VmPAcsIsClientSideScript.ScriptState.ScriptWait => "ScriptWait",
            VmPAcsIsClientSideScript.ScriptState.PleaseRemove => "PleaseRemove",
            _ => "Running",
        };

    public static int ScriptStateNameIndex(VmPAcsIsClientSideScript.ScriptState state) =>
        state switch
        {
            VmPAcsIsClientSideScript.ScriptState.Running => 0,
            VmPAcsIsClientSideScript.ScriptState.Suspended => 1,
            VmPAcsIsClientSideScript.ScriptState.Delayed => 2,
            VmPAcsIsClientSideScript.ScriptState.TagWait => 3,
            VmPAcsIsClientSideScript.ScriptState.PolyWait => 4,
            VmPAcsIsClientSideScript.ScriptState.ScriptWaitPre => 5,
            VmPAcsIsClientSideScript.ScriptState.ScriptWait => 6,
            VmPAcsIsClientSideScript.ScriptState.PleaseRemove => 7,
            _ => 0,
        };

    public static bool ResolveDeferredPlayerNumOnAdd(
        object? activator,
        object? player,
        int levelPlayerNum,
        out int playerNum)
    {
        playerNum = VmPAcsDLevelScriptCtor950Slice.ResolveDeferredPlayerNum(activator, player, levelPlayerNum);
        return playerNum != MissingDeferredPlayerNum;
    }
}

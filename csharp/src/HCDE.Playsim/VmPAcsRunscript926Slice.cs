namespace HCDE.Playsim;

/// <summary>
/// <c>CharArrayParms</c> tail, <c>GetClassForIndex</c>, and <c>DLevelScript::RunScript</c> head from <c>p_acs.cpp</c> (**7066–7215**); chunk **926**.
/// </summary>
public static class VmPAcsRunscript926Slice
{
    public const int RunawayInstructionLimit = 2_000_000;
    public const int LittleEnhancedEscapeBase = 256 - 16;
    public const int DefaultResultValue = 1;

    public enum WaitTickOutcome
    {
        Continue,
        ReturnResult,
    }

    public static bool TryPopCharArrayParms(List<int> stack, ref int sp, bool ranged, out int capacity, out int offset, out int index)
    {
        capacity = 0;
        offset = 0;
        index = 0;
        if (sp < (ranged ? 4 : 2)) return false;
        if (ranged)
        {
            capacity = stack[sp - 1];
            offset = stack[sp - 2];
            if (capacity < 1 || offset < 0)
            {
                sp -= 4;
                return false;
            }
            sp -= 2;
        }
        else
        {
            capacity = int.MaxValue;
            offset = 0;
        }
        index = stack[sp - 1];
        offset += stack[sp - 2];
        sp -= 2;
        return true;
    }

    public static string? LookupActorClassName(Func<int, string?> lookupString, int index) =>
        lookupString(index);

    public static int ComputeSpecialArgMask(bool levelHexenHack, AcsFormat format) =>
        levelHexenHack && format == AcsFormat.Old ? 255 : -1;

    public static bool TrySelectModuleLocalArrays(
        int inModuleScriptNumber,
        Func<int, bool> hasScriptPtr,
        out bool useModuleArrays)
    {
        useModuleArrays = false;
        if (inModuleScriptNumber < 0) return true;
        if (!hasScriptPtr(inModuleScriptNumber)) return false;
        useModuleArrays = true;
        return true;
    }

    public static WaitTickOutcome TickPreRunWaitState(
        VmPAcsIsClientSideScript.ScriptState state,
        ref VmPAcsIsClientSideScript.ScriptState nextState,
        ref int stateData,
        Func<int, bool> anyTaggedSectorBusy,
        Func<int, bool> polyObjectBusy,
        Func<int, bool> scriptStillRunning,
        Action? putFirst)
    {
        switch (state)
        {
            case VmPAcsIsClientSideScript.ScriptState.Delayed:
                if (--stateData == 0)
                    nextState = VmPAcsIsClientSideScript.ScriptState.Running;
                return WaitTickOutcome.Continue;

            case VmPAcsIsClientSideScript.ScriptState.TagWait:
                if (anyTaggedSectorBusy(stateData))
                    return WaitTickOutcome.ReturnResult;
                nextState = VmPAcsIsClientSideScript.ScriptState.Running;
                return WaitTickOutcome.Continue;

            case VmPAcsIsClientSideScript.ScriptState.PolyWait:
                if (!polyObjectBusy(stateData))
                    nextState = VmPAcsIsClientSideScript.ScriptState.Running;
                return WaitTickOutcome.Continue;

            case VmPAcsIsClientSideScript.ScriptState.ScriptWaitPre:
                if (scriptStillRunning(stateData))
                    nextState = VmPAcsIsClientSideScript.ScriptState.ScriptWait;
                return WaitTickOutcome.Continue;

            case VmPAcsIsClientSideScript.ScriptState.ScriptWait:
                if (scriptStillRunning(stateData))
                    return WaitTickOutcome.ReturnResult;
                nextState = VmPAcsIsClientSideScript.ScriptState.Running;
                putFirst?.Invoke();
                return WaitTickOutcome.Continue;

            default:
                return WaitTickOutcome.Continue;
        }
    }

    public static bool ShouldTerminateRunaway(bool scriptBusyFlagSet, ref uint runawayCounter)
    {
        if (scriptBusyFlagSet) return false;
        runawayCounter++;
        return runawayCounter > RunawayInstructionLimit;
    }

    public static int ReadNextPcode(ReadOnlySpan<byte> scriptBytes, ref int pc, AcsFormat format)
    {
        if (format == AcsFormat.LittleEnhanced)
        {
            var pcd = (int)scriptBytes[pc++];
            if (pcd >= LittleEnhancedEscapeBase)
                pcd = LittleEnhancedEscapeBase + ((pcd - LittleEnhancedEscapeBase) << 8) + scriptBytes[pc++];
            return pcd;
        }
        var word = scriptBytes[pc] | (scriptBytes[pc + 1] << 8);
        pc += 2;
        return word;
    }

    public static VmPAcsIsClientSideScript.ScriptState ApplyRunningOpcode(
        int pcd,
        bool treatAsUnknown,
        ref VmPAcsIsClientSideScript.ScriptState state)
    {
        if (treatAsUnknown || pcd == (int)HCDE.MapLoader.AcsPcode.Terminate)
        {
            state = VmPAcsIsClientSideScript.ScriptState.PleaseRemove;
            return state;
        }
        switch (pcd)
        {
            case (int)HCDE.MapLoader.AcsPcode.Nop:
                return state;
            case (int)HCDE.MapLoader.AcsPcode.Suspend:
                state = VmPAcsIsClientSideScript.ScriptState.Suspended;
                return state;
            default:
                state = VmPAcsIsClientSideScript.ScriptState.PleaseRemove;
                return state;
        }
    }
}

namespace HCDE.Playsim;

/// <summary>
/// `SetStop`, `SetWait`, `SetLoop`, `AddStates`, and `FinishStates` head from <c>p_states.cpp</c> (**901–1050**).
/// </summary>
public static class VmPStatesParser991Slice
{
    public const int StateDefineStop = 2;
    public const int StateDefineWait = 3;
    public const int StateDefineIndex = 5;

    public const int StateFlagNoDelay = 8;
    public const int StateFlagSameFrame = 16;

    public const int MaxFrameIndex = 28;
    public const int CaretFrameIndex = '\\' - 'A';

    public static bool TrySetStopOnParserSession(
        object? lastState,
        int lastLabel,
        object? lastStateBeforeLabel,
        Action<object> applyStopToState,
        Action<int> retargetFromLabelWithStop,
        Action<object?> clearLastStateBeforeLabel)
    {
        if (lastState is not null)
        {
            applyStopToState(lastState);
            clearLastStateBeforeLabel(null);
            return true;
        }

        if (lastLabel >= 0)
        {
            retargetFromLabelWithStop(lastLabel + 1);
            if (lastStateBeforeLabel is not null)
            {
                applyStopToState(lastStateBeforeLabel);
                clearLastStateBeforeLabel(null);
            }

            return true;
        }

        return false;
    }

    public static bool TrySetWaitOnParserSession(
        object? lastState,
        Action<object> applyWaitToState,
        Action<object?> clearLastStateBeforeLabel)
    {
        if (lastState is null)
            return false;

        applyWaitToState(lastState);
        clearLastStateBeforeLabel(null);
        return true;
    }

    public static bool TrySetLoopOnParserSession(
        object? lastState,
        int lastLabel,
        Action<object, int> applyLoopToState,
        Action<object?> clearLastStateBeforeLabel)
    {
        if (lastState is null)
            return false;

        applyLoopToState(lastState, lastLabel + 1);
        clearLastStateBeforeLabel(null);
        return true;
    }

    public static bool TryDecodeFrameCharacter(
        char frameChar,
        ref int frame,
        out bool noFrame)
    {
        noFrame = false;
        if (frameChar == '#')
        {
            noFrame = true;
            return true;
        }

        if (frameChar == '^')
            frame = CaretFrameIndex;
        else
            frame = (char.ToUpperInvariant(frameChar) & 223) - 'A';

        if (frame < 0 || frame > MaxFrameIndex)
        {
            frame = 0;
            return false;
        }

        return true;
    }

    public static void ApplyFrameToState(IMutableFrameState state, int frame, bool noFrame)
    {
        state.Frame = frame;
        if (noFrame)
            state.StateFlags |= StateFlagSameFrame;
        else
            state.StateFlags &= ~StateFlagSameFrame;
    }

    public static int AddStatesFromFrameCharacters(
        ReadOnlySpan<char> frameCharacters,
        IMutableFrameState workingState,
        Action<IMutableFrameState> pushStateSnapshot,
        Action<IMutableFrameState> clearNoDelayFlag)
    {
        var error = false;
        var frame = 0;
        var count = 0;
        for (var i = 0; i < frameCharacters.Length; i++)
        {
            if (!TryDecodeFrameCharacter(frameCharacters[i], ref frame, out var noFrame))
                error = true;

            ApplyFrameToState(workingState, frame, noFrame);
            pushStateSnapshot(workingState);
            count++;
            clearNoDelayFlag(workingState);
        }

        return error ? -count : count;
    }

    public interface IMutableFrameState
    {
        int Frame { get; set; }
        int StateFlags { get; set; }
        int DefineFlags { get; set; }
        int LoopTargetIndex { get; set; }
    }

    public static bool ShouldMaterializeOwnedStates(int stateCount) => stateCount > 0;

    public static void RunFinishStatesLabelFixPipeline(
        int stateCount,
        Action fixIndexedLabelPointers,
        Action resolvePendingGotoLabels)
    {
        if (stateCount <= 0)
        {
            resolvePendingGotoLabels();
            return;
        }

        fixIndexedLabelPointers();
        resolvePendingGotoLabels();
    }

    public static bool ShouldEnterPerStateDefineResolveLoop(int stateCount, int loopIndex) =>
        stateCount > 0 && loopIndex == 0;
}

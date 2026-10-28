namespace HCDE.Playsim;

/// <summary><c>SetAnimationInternal</c> from <c>p_actionfunctions.cpp</c> (6507–6676).</summary>
public static class VmActionSetAnimationInternal
{
    public const int ModelAnimNone = 0;
    public const int SafNooverride = 1;
    public const int SafInstant = 2;
    public const int SafLoop = 4;
    public const int Mf9Decoupledanimations = 1;

    public static bool RequiresDecoupledAnimations(int flags9) => (flags9 & Mf9Decoupledanimations) != 0;

    public static int NormalizeInterpolateTics(int interpolateTics) =>
        interpolateTics <= 0 ? 1 : interpolateTics;

    public static bool IsClearAnimation(string animName) =>
        string.IsNullOrEmpty(animName);

    public static bool AlreadyNone(int curFlags) => (curFlags & ModelAnimNone) != 0;

    public static bool ShouldClearToNone(bool clearRequest, int curFlags) =>
        clearRequest && !AlreadyNone(curFlags);

    public static bool AnimationNotFound(int findResult, int notFound) => findResult == notFound;

    public static bool SameAnimationBlocked(int flags, int curFlags, int curFirst, int animStart) =>
        (flags & SafNooverride) != 0 && curFlags != ModelAnimNone && curFirst == animStart;

    public static bool StartFramePastEnd(int startFrame, int len) => startFrame >= len;

    public static bool LoopFramePastEnd(int loopFrame, int len) => loopFrame >= len;

    public static bool EndFramePastEnd(int endFrame, int len) => endFrame >= len;

    public static bool UseInstantPath(int flags) => (flags & SafInstant) != 0;

    public static bool UseLoopFlag(int flags) => (flags & SafLoop) != 0;

    public static int ResolveLastFrame(int endFrame, int animEnd, int animStart) =>
        endFrame < 0 ? animEnd - 1 : animStart + endFrame;

    public static int ResolveStartFrame(int startFrame, int animStart) =>
        startFrame < 0 ? animStart : animStart + startFrame;

    public static int ResolveLoopFrame(int loopFrame, int animStart) =>
        loopFrame < 0 ? animStart : animStart + loopFrame;

    public static int ComputeStartTic(int floorTic, int interpolateTics) => floorTic + interpolateTics;

    public static double ComputeSwitchOffset(int startTic, double tic) => startTic - tic;

    public static bool ShouldUpdateInterpolation(double startTicMinusOffset, int floorTic) =>
        startTicMinusOffset != floorTic;

    public static bool StartTicInFuture(double startTic, double tic) => startTic > tic;

    public static bool NegativeFramerateUsesModel(double framerate) => framerate < 0;

    public static int AnimationLength(int animEnd, int animStart) => animEnd - animStart;

    public static bool WorldPausedOrFrozen(bool worldPaused, bool levelFrozen) =>
        worldPaused || levelFrozen;

    public static double ModelTimerWithFrac(double tic, double ticFrac, bool pausedOrFrozen) =>
        pausedOrFrozen ? tic : tic + ticFrac;

    public static int LoopFlagsFromSaf(int flags) => UseLoopFlag(flags) ? 1 : 0;

    public static bool ClearCurAnimToNone(int curFlags) => (curFlags & ModelAnimNone) == 0;

    public static bool MissingModelDef(bool hasModelData, bool hasModelDef) =>
        !(hasModelData && hasModelDef);

    public static int NotFoundResult(int errNotFound, int result) => result == errNotFound ? 1 : 0;

    public static double FloorTic(double tic) => Math.Floor(tic);

    public static int IntFloorTic(double tic) => (int)Math.Floor(tic);

    public static bool InterpSameTic(int startTic, int switchOffset, int floorTic) =>
        (startTic - switchOffset) == floorTic;

    public static bool ResetPrevAnimToInterp(bool instant) => !instant;

    public static float FramerateAsFloat(double framerate) => (float)framerate;

    public static int ModelAnimLoopFlag() => 1;

    public enum SetAnimationEarlyExit
    {
        Continue,
        ReturnFalseAlreadyNone,
        ReturnFalseAfterClear,
        ReturnFalseSameAnim,
        ReturnFalseNotFound,
        ReturnFalseBadFrame,
        Success,
    }

    public enum FrameBoundsViolation
    {
        None,
        Start,
        Loop,
        End,
    }

    public static SetAnimationEarlyExit TryHandleClearAnimation(string animName, int curFlags)
    {
        if (!IsClearAnimation(animName))
            return SetAnimationEarlyExit.Continue;
        if (AlreadyNone(curFlags))
            return SetAnimationEarlyExit.ReturnFalseAlreadyNone;
        return SetAnimationEarlyExit.ReturnFalseAfterClear;
    }

    public static FrameBoundsViolation CheckAnimationFrameBounds(int startFrame, int loopFrame, int endFrame, int len)
    {
        if (StartFramePastEnd(startFrame, len))
            return FrameBoundsViolation.Start;
        if (LoopFramePastEnd(loopFrame, len))
            return FrameBoundsViolation.Loop;
        if (EndFramePastEnd(endFrame, len))
            return FrameBoundsViolation.End;
        return FrameBoundsViolation.None;
    }

    public readonly record struct ResolvedAnimationFrames(
        int FirstFrame,
        int LastFrame,
        int StartFrame,
        int LoopFrame,
        int LoopFlags,
        float Framerate,
        double StartTic,
        double SwitchOffset);

    public static double ResolveModelTic(double tic, double ticFrac, bool worldPaused, bool levelFrozen) =>
        ModelTimerWithFrac(tic, ticFrac, WorldPausedOrFrozen(worldPaused, levelFrozen));

    public static double ResolveAnimationFramerate(double requested, double modelFramerate) =>
        NegativeFramerateUsesModel(requested) ? modelFramerate : requested;

    public static ResolvedAnimationFrames BuildResolvedAnimation(
        int animStart,
        int animEnd,
        int startFrame,
        int loopFrame,
        int endFrame,
        int flags,
        double framerate,
        double tic,
        int interpolateTics)
    {
        var instant = UseInstantPath(flags);
        var floorTic = IntFloorTic(tic);
        if (instant)
            return new ResolvedAnimationFrames(
                animStart,
                ResolveLastFrame(endFrame, animEnd, animStart),
                ResolveStartFrame(startFrame, animStart),
                ResolveLoopFrame(loopFrame, animStart),
                LoopFlagsFromSaf(flags),
                FramerateAsFloat(framerate),
                tic,
                0);
        var startTic = ComputeStartTic(floorTic, interpolateTics);
        return new ResolvedAnimationFrames(
            animStart,
            ResolveLastFrame(endFrame, animEnd, animStart),
            ResolveStartFrame(startFrame, animStart),
            ResolveLoopFrame(loopFrame, animStart),
            LoopFlagsFromSaf(flags),
            FramerateAsFloat(framerate),
            startTic,
            ComputeSwitchOffset(startTic, tic));
    }

    public static bool ShouldCapturePrevAnimInterpolation(int flags, double startTic, double switchOffset, double tic) =>
        !UseInstantPath(flags)
        && ShouldUpdateInterpolation(startTic - switchOffset, IntFloorTic(tic))
        && StartTicInFuture(startTic, tic);

    public readonly record struct SetAnimationGateInput(
        int Flags9,
        bool HasBaseSpriteModelFrame,
        string AnimName,
        int FindResult,
        int NotFound,
        int CurFlags,
        int CurFirstFrame,
        int AnimStart,
        int AnimEnd,
        int StartFrame,
        int LoopFrame,
        int EndFrame,
        int Flags,
        double Framerate,
        double ModelFramerate,
        double Tic,
        double TicFrac,
        bool WorldPaused,
        bool LevelFrozen,
        int InterpolateTics);

    public static (SetAnimationEarlyExit Exit, ResolvedAnimationFrames Frames) RunSetAnimationInternal(
        SetAnimationGateInput input)
    {
        if (!RequiresDecoupledAnimations(input.Flags9))
            return (SetAnimationEarlyExit.ReturnFalseBadFrame, default);
        if (MissingModelDef(true, input.HasBaseSpriteModelFrame))
            return (SetAnimationEarlyExit.ReturnFalseBadFrame, default);

        var interpolate = NormalizeInterpolateTics(input.InterpolateTics);
        var clear = TryHandleClearAnimation(input.AnimName, input.CurFlags);
        if (clear != SetAnimationEarlyExit.Continue)
            return (clear, default);

        if (AnimationNotFound(input.FindResult, input.NotFound))
            return (SetAnimationEarlyExit.ReturnFalseNotFound, default);

        if (SameAnimationBlocked(input.Flags, input.CurFlags, input.CurFirstFrame, input.AnimStart))
            return (SetAnimationEarlyExit.ReturnFalseSameAnim, default);

        var framerate = ResolveAnimationFramerate(input.Framerate, input.ModelFramerate);
        var len = AnimationLength(input.AnimEnd, input.AnimStart);
        if (CheckAnimationFrameBounds(input.StartFrame, input.LoopFrame, input.EndFrame, len) != FrameBoundsViolation.None)
            return (SetAnimationEarlyExit.ReturnFalseBadFrame, default);

        var tic = ResolveModelTic(input.Tic, input.TicFrac, input.WorldPaused, input.LevelFrozen);
        var frames = BuildResolvedAnimation(
            input.AnimStart,
            input.AnimEnd,
            input.StartFrame,
            input.LoopFrame,
            input.EndFrame,
            input.Flags,
            framerate,
            tic,
            interpolate);
        return (SetAnimationEarlyExit.Success, frames);
    }
}

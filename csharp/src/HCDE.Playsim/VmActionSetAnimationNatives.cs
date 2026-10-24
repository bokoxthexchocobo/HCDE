namespace HCDE.Playsim;

public static class VmActionSetAnimationNatives
{
    public const double ActionWrapperTicFrac = 1.0;

    public static void GetObjectToWorldMatrixRawNative(bool outMatrixNonNull, bool setGetBoneSharedOk, out bool shouldFillMatrix)
    {
        shouldFillMatrix = false;
        if (!outMatrixNonNull || !setGetBoneSharedOk)
            return;
        shouldFillMatrix = true;
    }

    public static void SetAnimationNative(
        int animNameIndex,
        double framerate,
        int startFrame,
        int loopFrame,
        int endFrame,
        int interpolateTics,
        int flags,
        out int normalizedInterpolateTics,
        out double ticFracForInternal) =>
        PrepareSetAnimationCall(animNameIndex, framerate, startFrame, loopFrame, endFrame, interpolateTics, flags, ActionWrapperTicFrac, out normalizedInterpolateTics, out ticFracForInternal);

    public static void SetAnimationUiNative(
        int animNameIndex,
        double framerate,
        int startFrame,
        int loopFrame,
        int endFrame,
        int interpolateTics,
        int flags,
        double uiTicFrac,
        out int normalizedInterpolateTics,
        out double ticFracForInternal) =>
        PrepareSetAnimationCall(animNameIndex, framerate, startFrame, loopFrame, endFrame, interpolateTics, flags, uiTicFrac, out normalizedInterpolateTics, out ticFracForInternal);

    private static void PrepareSetAnimationCall(
        int animNameIndex,
        double framerate,
        int startFrame,
        int loopFrame,
        int endFrame,
        int interpolateTics,
        int flags,
        double ticFrac,
        out int normalizedInterpolateTics,
        out double ticFracForInternal)
    {
        _ = (animNameIndex, framerate, startFrame, loopFrame, endFrame, flags);
        normalizedInterpolateTics = VmActionSetAnimationInternal.NormalizeInterpolateTics(interpolateTics);
        ticFracForInternal = ticFrac;
    }

    public static bool SetAnimationFrameRateInternalNative(
        bool selfNull,
        int flags9,
        bool hasModelData,
        double framerate,
        double ticFrac,
        bool worldPaused,
        bool levelFrozen,
        int curAnimFlags,
        double modelTimer,
        double curStartTic,
        out bool shouldThrowNotDecoupled,
        out bool shouldThrowNegativeRate,
        out bool earlyReturnNoneAnim,
        out double sampleTic,
        out bool shouldClearPrevAnim)
    {
        shouldThrowNotDecoupled = selfNull;
        if (selfNull)
        {
            shouldThrowNegativeRate = false;
            earlyReturnNoneAnim = false;
            sampleTic = 0;
            shouldClearPrevAnim = false;
            return false;
        }

        shouldThrowNotDecoupled = !VmActionSetAnimationInternal.RequiresDecoupledAnimations(flags9);
        if (!VmActionSetAnimationInternal.RequiresDecoupledAnimations(flags9))
        {
            shouldThrowNegativeRate = false;
            earlyReturnNoneAnim = false;
            sampleTic = 0;
            shouldClearPrevAnim = false;
            return false;
        }

        _ = hasModelData;
        earlyReturnNoneAnim = VmActionSetAnimationInternal.AlreadyNone(curAnimFlags);
        if (earlyReturnNoneAnim)
        {
            shouldThrowNegativeRate = false;
            sampleTic = 0;
            shouldClearPrevAnim = false;
            return false;
        }

        shouldThrowNegativeRate = framerate < 0;
        if (framerate < 0)
        {
            sampleTic = 0;
            shouldClearPrevAnim = false;
            return false;
        }

        sampleTic = VmActionSetAnimationInternal.ModelTimerWithFrac(
            modelTimer,
            ticFrac,
            VmActionSetAnimationInternal.WorldPausedOrFrozen(worldPaused, levelFrozen));

        shouldClearPrevAnim = curStartTic < sampleTic;
        return true;
    }

    public static void SetAnimationFrameRateNative(double framerate, out double ticFracUsed) =>
        SetAnimationFrameRateWrapper(framerate, ActionWrapperTicFrac, out ticFracUsed);

    public static void SetAnimationFrameRateUiNative(double framerate, double uiTicFrac, out double ticFracUsed) =>
        SetAnimationFrameRateWrapper(framerate, uiTicFrac, out ticFracUsed);

    private static void SetAnimationFrameRateWrapper(double framerate, double ticFrac, out double ticFracUsed)
    {
        _ = framerate;
        ticFracUsed = ticFrac;
    }

    public static bool AnimLayerToAnimInfoNative(
        bool layerPresent,
        bool curAnimPresent,
        int firstFrame,
        int lastFrame,
        int loopFrame,
        float layerFramerate,
        double startFrame,
        int flags,
        double startTic,
        double switchOffset,
        bool prevIsPrecalculated,
        bool prevIsInterpolated,
        out int curFlags,
        out bool hasPrevAnim)
    {
        if (!layerPresent || !curAnimPresent)
        {
            curFlags = VmActionSetAnimationInternal.ModelAnimNone;
            hasPrevAnim = false;
            return false;
        }

        curFlags = flags;
        _ = (firstFrame, lastFrame, loopFrame, layerFramerate, startFrame, startTic, switchOffset);
        hasPrevAnim = prevIsPrecalculated || prevIsInterpolated;
        return true;
    }
}

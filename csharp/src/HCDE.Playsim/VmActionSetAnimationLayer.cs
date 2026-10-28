namespace HCDE.Playsim;

public static class VmActionSetAnimationLayer
{
    public static bool CurAnimIsNone(int flags, int modelAnimNone) => (flags & modelAnimNone) != 0;

    public static bool SetAnimationLayerAnimationNative(
        bool layerNull,
        bool setAnimationInternalOk,
        bool tmpSizeGreaterThanZero,
        out bool shouldRestoreLayer,
        out bool shouldMoveTmpToFrame)
    {
        shouldRestoreLayer = false;
        shouldMoveTmpToFrame = false;
        if (layerNull)
            return false;
        if (!setAnimationInternalOk)
        {
            shouldRestoreLayer = true;
            return false;
        }

        shouldMoveTmpToFrame = tmpSizeGreaterThanZero;
        return true;
    }

    public static bool SetAnimationLayerAnimationUiNative(
        bool layerNull,
        bool setAnimationInternalOk,
        bool tmpSizeGreaterThanZero,
        out bool shouldRestoreLayer,
        out bool shouldMoveTmpToFrame) =>
        SetAnimationLayerAnimationNative(layerNull, setAnimationInternalOk, tmpSizeGreaterThanZero, out shouldRestoreLayer, out shouldMoveTmpToFrame);

    public static bool SetAnimationLayerFrameRateNative(
        bool layerNull,
        bool prevIsPrecalculated,
        bool nativePrevIsPrecalcIqm,
        out bool shouldCopyFrameData)
    {
        if (layerNull)
        {
            shouldCopyFrameData = false;
            return false;
        }

        shouldCopyFrameData = prevIsPrecalculated && nativePrevIsPrecalcIqm;
        return true;
    }

    public static bool SetAnimationLayerFrameRateUiNative(
        bool layerNull,
        bool prevIsPrecalculated,
        bool nativePrevIsPrecalcIqm,
        out bool shouldCopyFrameData) =>
        SetAnimationLayerFrameRateNative(layerNull, prevIsPrecalculated, nativePrevIsPrecalcIqm, out shouldCopyFrameData);

    public static bool AnimInfoToAnimLayerNative(
        int curFlags,
        int modelAnimNone,
        int firstFrame,
        int lastFrame,
        int loopFrame,
        float framerate,
        double startFrame,
        double startTic,
        double switchOffset,
        bool prevIsPrecalculatedIqm,
        bool prevIsInterpolated,
        float interpInter,
        int interpFrame1,
        int interpFrame2,
        out bool createCurAnimSequence,
        out bool clearCurAnim,
        out bool createPrecalcPrev,
        out bool createInterpPrev,
        out bool clearPrevAnim)
    {
        createCurAnimSequence = false;
        clearCurAnim = false;
        createPrecalcPrev = false;
        createInterpPrev = false;
        clearPrevAnim = false;

        if (!CurAnimIsNone(curFlags, modelAnimNone))
        {
            createCurAnimSequence = true;
            _ = (firstFrame, lastFrame, loopFrame, framerate, startFrame, startTic, switchOffset, curFlags);
        }
        else
        {
            clearCurAnim = true;
        }

        if (prevIsPrecalculatedIqm)
            createPrecalcPrev = true;
        else if (prevIsInterpolated)
        {
            createInterpPrev = true;
            _ = (interpInter, interpFrame1, interpFrame2);
        }
        else
            clearPrevAnim = true;

        return createCurAnimSequence || clearCurAnim;
    }

    public static void RestoreAnimLayerNative(
        bool layerPresent,
        bool layerPrevIsPrecalculatedFrame,
        bool infoPrevIsPrecalculatedIqm,
        out bool shouldMovePrecalcBonesBack)
    {
        shouldMovePrecalcBonesBack = layerPresent && layerPrevIsPrecalculatedFrame && infoPrevIsPrecalculatedIqm;
    }
}

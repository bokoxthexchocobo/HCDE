namespace HCDE.Playsim;

/// <summary>SetBones / OverwriteBones DEFINE policy (<c>p_actionfunctions.cpp</c> 7575–7809).</summary>
public static class VmActionSetBonesOverrides
{
    public static bool NeedsBoneOverrideSlot(int overrideSize) => overrideSize <= 0;

    public static int DefaultOverrideSlotCount() => 1;

    public static bool ResizeOverridesToJoints(int jointCount) => jointCount > 0;

    public static double SetBonesTic(double modelTimer) => modelTimer + 1.0;

    public static double SetBonesUiTic(double modelTimer, double frac) => modelTimer + frac;

    public static int MinBoneLoopCount(int frameCount, int overrideCount) =>
        frameCount < overrideCount ? frameCount : overrideCount;

    public static int RangeLoopEnd(int frameCount, int overrideCount, int start, int length)
    {
        int cap = start + length;
        int m = frameCount < overrideCount ? frameCount : overrideCount;
        return cap < m ? cap : m;
    }

    public static bool ShouldCalcBonesAfterSet(int n) => n > 0;

    public static bool UiSkipsCalcBones() => true;

    public static bool MaskSelectsBone(bool maskValue) => maskValue;

    public static bool RangeLoopInBounds(int i, int n, int start) => i >= start && i < n;

    public static bool OverwriteModeUsesMode(int mode) => mode >= 0;

    public static bool ForceRecalculateInvokesCalcBones(bool selfNonNull) => selfNonNull;

    public static bool SetModelFlagNativeForwards(bool selfNonNull) => selfNonNull;

    public static bool GetRenderStyleScansStyles(int styleCount) => styleCount > 0;

    public static int RenderStyleNotFound() => -1;

    public static bool StylesMatch(int index, bool match) => match;

    public static bool GrowOverridesWhenEmpty(int size) => size <= 0;

    public static int JointCountFromModel(int joints) => joints;

    public static bool LoopStartsAtRangeStart(int i, int start) => i >= start;

    public static bool ApplyMaskInLoop(bool masked) => masked;

    public static bool NativeDefineReturnsZero() => true;

    public static bool ScanRenderStyleLoop(int i, int count) => i < count;

    public static bool ReturnSymbolicStyleIndex(bool matched) => matched;

    public static int OverrideSlotIndex() => 0;

    public static bool BonesFrameDataNonNull(bool nonNull) => nonNull;

    public static bool BonesObjectNonNull(bool nonNull) => nonNull;

    public static bool MaskPointerNonNull(bool nonNull) => nonNull;

    public static float InterpLengthPositive(float len) => len;

    public static int ModeFromParam(int mode) => mode;

    public static bool SetBonesUsesModelIndexZero() => true;

    public static bool OverwriteSkipsTic() => true;

    public static bool RangeUsesStartParam(int start) => start >= 0;

    public static bool RangeUsesLengthParam(int length) => length >= 0;

    public static bool UiCommentSkipsCalcBones(bool ui) => ui;

    public static bool ClearModelFlagNativeForwards(bool self) => self;

    public static bool ResetModelFlagsNativeForwards(bool self) => self;

    public static bool GetRenderStyleSelfNonNull(bool self) => self;

    public static int StyleCountUpperBound(int styleCount) => styleCount;

    public static bool LegacyStyleEquals(bool eq) => eq;

    public static bool ActionReturnIntWhenFound(bool found) => found;

    public static bool ActionReturnIntWhenMissing(bool found) => !found;

    public static bool SetPathUsesSharedModel(bool sharedOk) => sharedOk;

    public static int SharedModelBoneIndex() => 0;

    public static bool OverrideArrayEmpty(int size) => size <= 0;

    public static bool ResizeOverrideArrayToOne(int size) => size <= 0;

    public static bool MatchFrameAndOverrideSizes(int frames, int overrides) => frames > 0 && overrides > 0;

    public static bool IterateBoneIndex(int i, int n) => i < n;

    public static bool SetBoneOverrideAt(int i, int n) => i >= 0 && i < n;

    public static bool OverwriteBoneOverrideAt(int i, int n) => i >= 0 && i < n;

    public static bool MaskedSetAt(int i, bool mask) => mask;

    public static bool MaskedOverwriteAt(int i, bool mask) => mask;

    public static bool CalcBonesAfterMaskedSet(int n) => n > 0;

    public static bool UiPathSkipsCalc(bool ui) => ui;

    public static bool ParamBonesNotNull(bool bones) => bones;

    public static bool ParamMaskNotNull(bool mask) => mask;

    public static bool ParamFrameNotNull(bool frame) => frame;

    public static bool ParamSelfNotNull(bool self) => self;

    public static bool ReturnZeroFromNative() => true;

    public static bool ReturnPointerFromNative(bool ok) => ok;

    public static int ParamFlagValue(int flag) => flag;

    public static int ParamIqmFlagValue(int flag) => flag;

    public static bool ParamResetModel(bool v) => v;

    public static bool ParamResetIqm(bool v) => v;

    public static bool RenderStyleLoopContinue(int i, int count) => i < count;

    public static bool StyleScanIncrement(int i) => i >= 0;
}

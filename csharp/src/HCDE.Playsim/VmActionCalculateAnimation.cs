namespace HCDE.Playsim;

/// <summary>CalculateAnimation*, FindAnimationFrame*, blend/offset, SetBones* policy (<c>p_actionfunctions.cpp</c> 7246–7567).</summary>
public static class VmActionCalculateAnimation
{
    public static bool CalculateAnimationInternalNullModel(bool modelFound) => !modelFound;

    public static bool CalculateAnimationLayerNull(bool layerNull) => layerNull;

    public static double CalculateAnimationTic(double modelTimer, double ticFrac) => modelTimer + ticFrac;

    public static bool FindAnimationFrameNegativeTic(double tic) => tic < 0;

    public static bool ShouldCreateInterpFrame(bool layerNotNull, double tic) =>
        layerNotNull && tic >= 0;

    public static bool CalculateAnimationFrameNullIframe(bool iframeNull) => iframeNull;

    public static bool CalculateAnimationFrameNullModel(bool modelFound) => !modelFound;

    public static bool RestoreAnimLayerAfterCalculate(bool layerNonNull) => layerNonNull;

    public static double UiModelTimer(double tic, double frac, bool pausedOrFrozen) =>
        pausedOrFrozen ? tic : tic + frac;

    public static bool ReturnNullWhenLayerMissing(bool layerNull) => layerNull;

    public static bool ReturnNullWhenModelMissing(bool hasModel) => !hasModel;

    public static bool FindAnimationFrameAtSetsFloat(bool numretGt2) => numretGt2;

    public static bool FindAnimationFrameAtSetsObject(bool numretGt1) => numretGt1;

    public static bool CalcUsesModelTimerPlusOne(double timer) => timer >= 0;

    public static double UiFindTic(double timer, double frac, bool pausedOrFrozen) =>
        pausedOrFrozen ? timer : timer + frac;

    public static bool SetRetInterWhenThird(bool numretGt2) => numretGt2;

    public static bool BlendSizesMatch(int a, int b) => a == b;

    public static double BlendOneMinusT(double t) => 1.0 - t;

    public static bool ThrowWhenBlendSizesDiffer(bool match) => !match;

    public static bool ShouldInterpolateBone(int index, int count) => index < count;

    public static bool OffsetSizesMatch(int frameSize, int offsetSize) => frameSize == offsetSize;

    public static bool ThrowOffsetSizeMismatch(bool match) => !match;

    public static bool NeedsBoneOverrideSlot(int overrideSize) => overrideSize <= 0;

    public static int MinBoneLoopCount(int frameCount, int overrideCount) =>
        frameCount < overrideCount ? frameCount : overrideCount;

    public static double SetBonesTic(double modelTimer) => modelTimer + 1.0;

    public static double SetBonesUiTic(double modelTimer, double frac) => modelTimer + frac;

    public static bool ShouldCalcBonesAfterSet(int n) => n > 0;

    public static bool UiSkipsCalcBonesComment() => true;
}

namespace HCDE.Playsim;

/// <summary>FindAnimationFrameUI and BlendAnimationFrames policy (<c>p_actionfunctions.cpp</c> 7416–7565).</summary>
public static class VmActionFindAnimationFrame
{
    public static bool UiFindUsesFrozenAwareTic(bool worldPaused, bool levelFrozen) => worldPaused || levelFrozen;

    public static double UiFindTic(double timer, double frac, bool pausedOrFrozen) =>
        pausedOrFrozen ? timer : timer + frac;

    public static bool SetRetInterWhenThird(bool numretGt2) => numretGt2;

    public static bool SetRetInterpWhenSecond(bool numretGt1) => numretGt1;

    public static bool SetRetFrame1WhenFirst(bool numretGt0) => numretGt0;

    public static bool BlendSizesMatch(int a, int b) => a == b;

    public static double BlendOneMinusT(double t) => 1.0 - t;

    public static int BlendLoopCount(int n) => n;

    public static bool ShouldInterpolateBone(int i, int n) => i < n;

    public static bool ThrowWhenBlendSizesDiffer(bool match) => !match;

    public static bool ShouldInterpolateBone(int index, int count) => index < count;

    public static bool ResizeBlendOutput(int size) => size > 0;

    public static bool LoopAllBones(int i, int n) => i < n;

    public static bool AbortBlendOnBounds(bool sizesMatch) => !sizesMatch;

    public static int BlendOutputSize(int aSize) => aSize;

    public static float SelectInterp(float inter) => inter;

    public static int SelectFrame1Index(int frame1) => frame1;

    public static int SelectFrame2Index(int frame2) => frame2;

    public static bool UiWorldPaused(bool worldPaused, bool levelFrozen) => worldPaused || levelFrozen;

    public static double FrozenUiTic(double tic) => tic;

    public static double ActiveUiTic(double tic, double frac) => tic + frac;

    public static bool OffsetSizesMatch(int frameSize, int offsetSize) => frameSize == offsetSize;

    public static bool OffsetLoopInRange(int i, int n) => i < n;

    public static bool ThrowOffsetSizeMismatch(bool match) => !match;

    public static bool AllocateOffsetOutput(int size) => size > 0;

    public static bool OffsetFrameNonNull(bool frameNonNull) => frameNonNull;

    public static bool OffsetDataNonNull(bool offsetsNonNull) => offsetsNonNull;

    public static int OffsetBoneIndex(int i) => i;

    public static bool WriteOffsetBone(int i, int n) => i >= 0 && i < n;

    public static bool BlendBranchTaken(bool sizesMatch) => sizesMatch;

    public static bool ElseBranchThrows(bool sizesMatch) => !sizesMatch;

    public static double BlendParameterT(double t) => t;

    public static double BlendParameterInvT(double invT) => invT;

    public static bool ReturnBlendPointer(bool created) => created;

    public static int NumRetSlots(int numret) => numret;

    public static bool ShouldSetFloatRet(bool hasThird) => hasThird;

    public static bool ShouldSetObjectRet(bool hasSecond) => hasSecond;

    public static bool ShouldSetFrameRet(bool hasFirst) => hasFirst;
}

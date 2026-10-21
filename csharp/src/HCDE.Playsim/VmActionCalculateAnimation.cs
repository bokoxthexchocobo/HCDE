namespace HCDE.Playsim;

/// <summary>CalculateAnimation* and FindAnimationFrame* policy from <c>p_actionfunctions.cpp</c> (7246–7545).</summary>
public static class VmActionCalculateAnimation
{
    public static bool CalculateAnimationInternalNullModel(bool modelFound) => !modelFound;

    public static bool CalculateAnimationLayerNull(bool layerNull) => layerNull;

    public static double CalculateAnimationTic(double modelTimer, double ticFrac) => modelTimer + ticFrac;

    public static bool FindAnimationFrameLayerNull(bool layerNull) => layerNull;

    public static bool FindAnimationFrameNegativeTic(double tic) => tic < 0;

    public static bool ShouldCreateInterpFrame(bool layerNotNull, double tic) =>
        layerNotNull && tic >= 0;

    public static bool UsePrevAnimWhenInterPositive(float inter) => inter >= 0;

    public static bool FindAnimationFrameAtSetsFloat(bool numretGreaterThan2) => numretGreaterThan2;

    public static bool FindAnimationFrameAtSetsObject(bool numretGreaterThan1) => numretGreaterThan1;

    public static bool FindAnimationFrameAtSetsFrame1(bool numretGreaterThan0) => numretGreaterThan0;

    public static bool CalculateAnimationFrameNullIframe(bool iframeNull) => iframeNull;

    public static bool CalculateAnimationFrameNullModel(bool modelFound) => !modelFound;

    public static bool BlendFrameSizesMatch(int sizeA, int sizeB) => sizeA == sizeB;

    public static double BlendInverseT(double t) => 1.0 - t;

    public static int BlendBoneCount(int size) => size;

    public static bool OffsetFrameSizesMatch(int frameSize, int offsetSize) => frameSize == offsetSize;

    public static bool ShouldThrowSizeMismatch(bool sizesMatch) => !sizesMatch;

    public static bool RestoreAnimLayerAfterCalculate(bool layerNonNull) => layerNonNull;

    public static bool PrecalcFrameUsesInterp(float inter, int frame1, int frame2) =>
        inter >= 0 && (frame1 >= 0 || frame2 >= 0);

    public static bool UiTicAddsFrac(bool worldPaused, bool levelFrozen, double frac) =>
        !worldPaused && !levelFrozen && frac > 0;

    public static double UiModelTimer(double tic, double frac, bool pausedOrFrozen) =>
        pausedOrFrozen ? tic : tic + frac;

    public static bool InterpFrameFromPrevAnim(bool hasPrevAnim) => hasPrevAnim;

    public static int FrameIndexOrNegOne(int index) => index;

    public static float InterpOrNegOne(float inter) => inter;

    public static bool CalcUsesModelTimerPlusOne(double timer) => timer >= 0;

    public static bool ReturnNullWhenLayerMissing(bool layerNull) => layerNull;

    public static bool ReturnNullWhenIframeMissing(bool iframeNull) => iframeNull;

    public static bool ReturnNullWhenModelMissing(bool hasModel) => !hasModel;

    public static bool FindAnimationFrameUsesTimerPlusOne(bool selfNonNull) => selfNonNull;

    public static bool SetRetFloatWhenRequested(bool numretGt2) => numretGt2;

    public static bool SetRetObjectWhenRequested(bool numretGt1) => numretGt1;

    public static bool SetRetFrameWhenRequested(bool numretGt0) => numretGt0;

    public static bool WorldUnpausedForUi(bool worldPaused, bool levelFrozen) => !worldPaused && !levelFrozen;

    public static double AddUiFrac(double tic, double frac, bool addFrac) => addFrac ? tic + frac : tic;

    public static bool LayerHasCurAnim(bool layerNonNull, bool curAnimNonNull) => layerNonNull && curAnimNonNull;

    public static bool PrevAnimIsPrecalculated(bool isPrecalcClass) => isPrecalcClass;

    public static bool PrevAnimIsInterpolated(bool isInterpClass) => isInterpClass;

    public static bool CopyPrecalcBonesOnFrameRate(bool shouldCopy) => shouldCopy;
}

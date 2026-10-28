namespace HCDE.Playsim;

/// <summary>Precalculated animation calculate/find/blend bands from <c>p_actionfunctions.cpp</c> (7246–7512).</summary>
public static class VmActorPrecalcAnimation
{
    public static bool HasModelForPrecalc(object? model) => model is not null;

    public static bool ShouldAbortCalculateWhenLayerMissing(object? layer) => layer is null;

    public static double GameAnimationSampleTic(double modelTimer) => modelTimer + 1.0;

    public static double UiAnimationSampleTic(double modelTimer, double timeFrac) => modelTimer + timeFrac;

    public static bool ShouldAbortCalculateFrameWhenMissing(object? iframe) => iframe is null;

    public static bool ShouldFindAnimationFrame(bool hasLayer, double tic) => hasLayer && tic >= 0;

    public static bool ShouldUsePrevAnimFrame(float interpolation) => interpolation >= 0;

    public static double FindAnimationFrameUiTic(
        double modelTimer,
        bool worldPaused,
        bool levelFrozen,
        double timeFrac)
    {
        var tic = modelTimer;
        if (!worldPaused && !levelFrozen)
            tic += timeFrac;
        return tic;
    }

    public static bool CanBlendPrecalcFrameData(int sizeA, int sizeB) => sizeA == sizeB;

    public static double InverseBlendWeight(double t) => 1.0 - t;

    public static bool CanOffsetPrecalcFrameData(int frameSize, int offsetSize) => frameSize == offsetSize;

    public static (double X, double Y, double Z) OffsetBoneTranslation(
        double frameX,
        double frameY,
        double frameZ,
        double offX,
        double offY,
        double offZ) => (frameX + offX, frameY + offY, frameZ + offZ);

    public static (double X, double Y, double Z) MultiplyBoneScale(
        double frameX,
        double frameY,
        double frameZ,
        double offX,
        double offY,
        double offZ) => (frameX * offX, frameY * offY, frameZ * offZ);

    public static int ScanLegacyRenderStyleIndex(ReadOnlySpan<int> legacyStyles, int renderStyle)
    {
        for (var i = 0; i < legacyStyles.Length; i++)
        {
            if (legacyStyles[i] == renderStyle)
                return i;
        }

        return -1;
    }

    public static int BlendPrecalcBoneLoopUpperBound(int sizeA, int sizeB) =>
        CanBlendPrecalcFrameData(sizeA, sizeB) ? sizeA : 0;

    public static bool BlendThrowsOnSizeMismatch(int sizeA, int sizeB) => sizeA != sizeB;

    public static bool OffsetThrowsOnSizeMismatch(int frameSize, int offsetSize) => frameSize != offsetSize;

    public static int OffsetPrecalcBoneLoopUpperBound(int frameSize, int offsetSize) =>
        CanOffsetPrecalcFrameData(frameSize, offsetSize) ? frameSize : 0;

    public static float DefaultInterpolationWhenUnset => -1f;

    public static bool CalculateAnimationInternalUsesModelTimerPlusOne(double timer, double tic) => tic > timer;

    public static bool CalculateAnimationUiUsesTimeFrac(double timer, double tic, double frac) =>
        Math.Abs(tic - (timer + frac)) < 1e-6;

    public static bool FindAnimationFrameAtUsesExplicitTic(double suppliedTic, double usedTic) =>
        Math.Abs(suppliedTic - usedTic) < 1e-9;

    public static bool ShouldWriteFindAnimationRetSlot(int numRet, int slotIndex) => numRet > slotIndex;

    public static double InterpolateBoneInverseT(double t) => InverseBlendWeight(t);

    public static (double W, double X, double Y, double Z) NormalizeQuaternion(double w, double x, double y, double z)
    {
        var len = Math.Sqrt(w * w + x * x + y * y + z * z);
        if (len <= 0)
            return (1, 0, 0, 0);
        return (w / len, x / len, y / len, z / len);
    }

    public static (double W, double X, double Y, double Z) MultiplyQuaternion(
        double aw,
        double ax,
        double ay,
        double az,
        double bw,
        double bx,
        double by,
        double bz)
    {
        return (
            aw * bw - ax * bx - ay * by - az * bz,
            aw * bx + ax * bw + ay * bz - az * by,
            aw * by - ax * bz + ay * bw + az * bx,
            aw * bz + ax * by - ay * bx + az * bw);
    }

    public static (double W, double X, double Y, double Z) OffsetRotationMultiplyUnit(
        double frameW,
        double frameX,
        double frameY,
        double frameZ,
        double offW,
        double offX,
        double offY,
        double offZ)
    {
        var (w, x, y, z) = MultiplyQuaternion(frameW, frameX, frameY, frameZ, offW, offX, offY, offZ);
        return NormalizeQuaternion(w, x, y, z);
    }

    public static int GetRenderStyleUnknownIndex => -1;

    public static bool LegacyStyleTableExhausted(int index, int styleCount) => index >= styleCount;

    public static bool StyleEntryMatches(int legacyEntry, int actorStyle) => legacyEntry == actorStyle;

    public static bool BlendAnimationReturnsPointerOnSuccess(int sizeA, int sizeB) =>
        CanBlendPrecalcFrameData(sizeA, sizeB);

    public static bool OffsetAnimationReturnsPointerOnSuccess(int frameSize, int offsetSize) =>
        CanOffsetPrecalcFrameData(frameSize, offsetSize);

    public static float InterpolationUnset => DefaultInterpolationWhenUnset;

    public static bool FindAnimationUsesGameTic(double modelTimer, double tic) =>
        Math.Abs(tic - GameAnimationSampleTic(modelTimer)) < 1e-9;

    public static bool FindAnimationUiUsesAdjustedTic(
        double modelTimer,
        bool worldPaused,
        bool levelFrozen,
        double frac,
        double tic) =>
        Math.Abs(tic - FindAnimationFrameUiTic(modelTimer, worldPaused, levelFrozen, frac)) < 1e-9;
}

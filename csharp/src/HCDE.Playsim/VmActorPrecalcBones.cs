namespace HCDE.Playsim;

/// <summary>Set/overwrite bones and mask/range bands from <c>p_actionfunctions.cpp</c> (7514–7764).</summary>
public static class VmActorPrecalcBones
{
    public static bool NeedsBoneOverrideArrayResize(int overrideArraySize) => overrideArraySize <= 0;

    public static int BoneOverrideApplyCount(int frameBoneCount, int overrideSlotCount) =>
        Math.Min(frameBoneCount, overrideSlotCount);

    public static int RangeBoneApplyEnd(int frameBoneCount, int overrideSlotCount, int start, int length) =>
        Math.Min(frameBoneCount, Math.Min(overrideSlotCount, start + length));

    public static bool ShouldApplyMaskedBone(bool maskAtIndex) => maskAtIndex;

    public static bool ShouldRecalcBonesAfterApply(int appliedBoneCount, bool uiPath) =>
        appliedBoneCount > 0 && !uiPath;

    public static double GameBoneApplyTic(double modelTimer) => modelTimer + 1.0;

    public static double UiBoneApplyTic(double modelTimer, double timeFrac) => modelTimer + timeFrac;

    public static int CountMaskedBoneApplications(ReadOnlySpan<bool> mask, int boneCount)
    {
        var n = Math.Min(mask.Length, boneCount);
        var count = 0;
        for (var i = 0; i < n; i++)
        {
            if (mask[i])
                count++;
        }

        return count;
    }

    public static int IterateRangeBoneIndices(int start, int endExclusive)
    {
        var count = 0;
        for (var i = start; i < endExclusive; i++)
            count++;
        return count;
    }

    public static bool OverwriteBonesSkipsCalcBonesComment(bool uiPath) => uiPath;

    public static bool GrowBoneOverridesToModelJointCount(int currentSize, int jointCount, out int newSize)
    {
        newSize = jointCount;
        return currentSize != jointCount;
    }

    public static int MinBoneFrameAndOverride(int frameBones, int overrides, int start, int length) =>
        RangeBoneApplyEnd(frameBones, overrides, start, length);

    public static bool AnyBoneAppliedInRange(int start, int endExclusive) => endExclusive > start;

    public static bool MaskedSetRequiresPositiveMaskCount(int maskedCount) => maskedCount > 0;

    public static int ClampBoneLoopStart(int start, int overrideSlots) => start < 0 ? 0 : Math.Min(start, overrideSlots);

    public static bool UiSetBonesSkipsCalcBones(bool uiPath) => uiPath;

    public static bool GameSetBonesRunsCalcBones(int applied, bool uiPath) => ShouldRecalcBonesAfterApply(applied, uiPath);

    public static int SumMinApplyCounts(int frameCount, int overrideCount) =>
        BoneOverrideApplyCount(frameCount, overrideCount);

    public static bool OverwriteRangeWithinBounds(int start, int length, int frameBones, int overrideSlots)
    {
        var end = RangeBoneApplyEnd(frameBones, overrideSlots, start, length);
        return start >= 0 && end >= start;
    }

    public static bool MaskLengthCoversBones(ReadOnlySpan<bool> mask, int boneCount) => mask.Length >= boneCount;

    public static int MaskedLoopUpperBound(int frameBones, int overrideSlots) =>
        Math.Min(frameBones, overrideSlots);

    public static bool ShouldResizeSingleModelSlot(int modelSlotCount) => modelSlotCount <= 0;

    public static int DefaultModelSlotIndex => 0;

    public static bool ApplyBoneOverrideLoop(int index, int upperBound) => index < upperBound;

    public static int FullBoneApplyUpperBound(int frameBones, int overrideSlots) =>
        BoneOverrideApplyCount(frameBones, overrideSlots);

    public static bool SetBonesResizesOverridesWhenEmpty(int slotCount) => NeedsBoneOverrideArrayResize(slotCount);

    public static bool SetBonesResizesToJointCount(int jointCount, int currentOverrides) =>
        GrowBoneOverridesToModelJointCount(currentOverrides, jointCount, out _);

    public static int RangeLoopStartIndex(int start, int endExclusive) => start;

    public static bool RangeLoopContinues(int i, int endExclusive) => i < endExclusive;

    public static bool MaskLoopContinues(int i, int upperBound) => i < upperBound;

    public static bool MaskLoopAppliesWhenTrue(bool maskValue) => maskValue;

    public static bool OverwriteBonesUsesMode(int mode) => mode >= 0;

    public static bool SetBonesUsesInterpolationLength(double interplen) => interplen >= 0;

    public static int CompareFrameAndOverrideSizes(int frameBones, int overrideSlots) =>
        frameBones.CompareTo(overrideSlots);

    public static bool ModelBoneSlotZeroIsDefault(int modelIndex) => modelIndex == DefaultModelSlotIndex;

    public static int MinThreeWay(int a, int b, int c) => Math.Min(a, Math.Min(b, c));

    public static int RangeApplyIterationCount(int start, int endExclusive) =>
        IterateRangeBoneIndices(start, endExclusive);

    public static bool CalcBonesAfterSetWhenGameplay(int applied, bool ui) => GameSetBonesRunsCalcBones(applied, ui);

    public static bool CalcBonesSkippedForUiSet(int applied, bool ui) => UiSetBonesSkipsCalcBones(ui) && applied > 0;

    public static bool OverwriteMaskHonorsMaskArray(ReadOnlySpan<bool> mask, int index) =>
        index < mask.Length && mask[index];

    public static int MaskedUpperWithFrameAndOverride(int frameBones, int overrideSlots, ReadOnlySpan<bool> mask) =>
        Math.Min(MaskedLoopUpperBound(frameBones, overrideSlots), mask.Length);

    public static bool BoneOverrideSetUsesTic(double tic, double modeTic) => Math.Abs(tic - modeTic) < 1e-9;

    public static bool OverwriteSkipsInterpolation(bool overwritePath) => overwritePath;

    public static int SingleModelDataSlot => 1;

    public static bool ResizeModelBoneOverridesToOneWhenEmpty(int size) => size <= 0;

    public static int MaskedApplyUpperBound(int frameBones, int overrideSlots, int maskLength) =>
        Math.Min(MaskedLoopUpperBound(frameBones, overrideSlots), maskLength);

    public static bool RangeApplyIncludesStartIndex(int index, int start) => index >= start;

    public static double SelectBoneApplyTic(bool uiPath, double modelTimer, double timeFrac) =>
        uiPath ? UiBoneApplyTic(modelTimer, timeFrac) : GameBoneApplyTic(modelTimer);
}

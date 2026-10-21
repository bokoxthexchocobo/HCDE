namespace HCDE.Playsim;

public static class VmActorBoneTransformNatives
{
    public const int DefaultModelIndex = 0;
    public const double ActionWrapperTicFrac = 1.0;
    public const double GetBoneOffsetTicOffset = 1.0;

    public static int GetBoneOffsetSampleTics(double modelTimer) =>
        (int)(modelTimer + GetBoneOffsetTicOffset);

    public static int FinalizeGetBoneOffsetNumret(int numret) => numret > 2 ? 3 : numret;

    private static bool TryApplyBoneOverride(
        bool setBoneOffsetSharedReturnedModel,
        double modelTimer,
        double ticFrac,
        out double overrideSampleTime,
        out bool shouldCalcBones)
    {
        if (!setBoneOffsetSharedReturnedModel)
        {
            overrideSampleTime = 0;
            shouldCalcBones = false;
            return false;
        }

        overrideSampleTime = modelTimer + ticFrac;
        shouldCalcBones = true;
        return true;
    }

    public static void SetModelBoneRotationNative(
        bool setBoneOffsetSharedReturnedModel,
        double modelTimer,
        int modelIndex,
        int boneIndex,
        double rotX,
        double rotY,
        double rotZ,
        double rotW,
        int mode,
        double interpolationDuration,
        double ticFrac,
        out double rotationOverrideSampleTime,
        out bool shouldCalcBones)
    {
        if (!TryApplyBoneOverride(setBoneOffsetSharedReturnedModel, modelTimer, ticFrac, out rotationOverrideSampleTime, out shouldCalcBones))
            return;
        _ = (modelIndex, boneIndex, rotX, rotY, rotZ, rotW, mode, interpolationDuration);
    }

    public static void SetBoneRotationNative(
        bool setBoneOffsetSharedReturnedModel,
        double modelTimer,
        int boneIndex,
        double rotX,
        double rotY,
        double rotZ,
        double rotW,
        int mode,
        double interpolationDuration,
        double ticFrac,
        out double rotationOverrideSampleTime,
        out bool shouldCalcBones) =>
        SetModelBoneRotationNative(
            setBoneOffsetSharedReturnedModel,
            modelTimer,
            DefaultModelIndex,
            boneIndex,
            rotX,
            rotY,
            rotZ,
            rotW,
            mode,
            interpolationDuration,
            ticFrac,
            out rotationOverrideSampleTime,
            out shouldCalcBones);

    public static void SetModelNamedBoneRotationNative(
        bool setBoneOffsetSharedReturnedModel,
        double modelTimer,
        int modelIndex,
        int boneNameIndex,
        int resolvedBoneIndex,
        double rotX,
        double rotY,
        double rotZ,
        double rotW,
        int mode,
        double interpolationDuration,
        double ticFrac,
        out double rotationOverrideSampleTime,
        out bool shouldCalcBones)
    {
        _ = boneNameIndex;
        SetModelBoneRotationNative(
            setBoneOffsetSharedReturnedModel,
            modelTimer,
            modelIndex,
            resolvedBoneIndex,
            rotX,
            rotY,
            rotZ,
            rotW,
            mode,
            interpolationDuration,
            ticFrac,
            out rotationOverrideSampleTime,
            out shouldCalcBones);
    }

    public static void SetNamedBoneRotationNative(
        bool setBoneOffsetSharedReturnedModel,
        double modelTimer,
        int boneNameIndex,
        int resolvedBoneIndex,
        double rotX,
        double rotY,
        double rotZ,
        double rotW,
        int mode,
        double interpolationDuration,
        double ticFrac,
        out double rotationOverrideSampleTime,
        out bool shouldCalcBones) =>
        SetModelNamedBoneRotationNative(
            setBoneOffsetSharedReturnedModel,
            modelTimer,
            DefaultModelIndex,
            boneNameIndex,
            resolvedBoneIndex,
            rotX,
            rotY,
            rotZ,
            rotW,
            mode,
            interpolationDuration,
            ticFrac,
            out rotationOverrideSampleTime,
            out shouldCalcBones);

    public static void SetModelBoneTranslationNative(
        bool setBoneOffsetSharedReturnedModel,
        double modelTimer,
        int modelIndex,
        int boneIndex,
        double transX,
        double transY,
        double transZ,
        int mode,
        double interpolationDuration,
        double ticFrac,
        out double translationOverrideSampleTime,
        out bool shouldCalcBones)
    {
        if (!TryApplyBoneOverride(setBoneOffsetSharedReturnedModel, modelTimer, ticFrac, out translationOverrideSampleTime, out shouldCalcBones))
            return;
        _ = (modelIndex, boneIndex, transX, transY, transZ, mode, interpolationDuration);
    }

    public static void SetBoneTranslationNative(
        bool setBoneOffsetSharedReturnedModel,
        double modelTimer,
        int boneIndex,
        double transX,
        double transY,
        double transZ,
        int mode,
        double interpolationDuration,
        double ticFrac,
        out double translationOverrideSampleTime,
        out bool shouldCalcBones) =>
        SetModelBoneTranslationNative(
            setBoneOffsetSharedReturnedModel,
            modelTimer,
            DefaultModelIndex,
            boneIndex,
            transX,
            transY,
            transZ,
            mode,
            interpolationDuration,
            ticFrac,
            out translationOverrideSampleTime,
            out shouldCalcBones);

    public static void SetModelNamedBoneTranslationNative(
        bool setBoneOffsetSharedReturnedModel,
        double modelTimer,
        int modelIndex,
        int boneNameIndex,
        int resolvedBoneIndex,
        double transX,
        double transY,
        double transZ,
        int mode,
        double interpolationDuration,
        double ticFrac,
        out double translationOverrideSampleTime,
        out bool shouldCalcBones)
    {
        _ = boneNameIndex;
        SetModelBoneTranslationNative(
            setBoneOffsetSharedReturnedModel,
            modelTimer,
            modelIndex,
            resolvedBoneIndex,
            transX,
            transY,
            transZ,
            mode,
            interpolationDuration,
            ticFrac,
            out translationOverrideSampleTime,
            out shouldCalcBones);
    }

    public static void SetNamedBoneTranslationNative(
        bool setBoneOffsetSharedReturnedModel,
        double modelTimer,
        int boneNameIndex,
        int resolvedBoneIndex,
        double transX,
        double transY,
        double transZ,
        int mode,
        double interpolationDuration,
        double ticFrac,
        out double translationOverrideSampleTime,
        out bool shouldCalcBones) =>
        SetModelNamedBoneTranslationNative(
            setBoneOffsetSharedReturnedModel,
            modelTimer,
            DefaultModelIndex,
            boneNameIndex,
            resolvedBoneIndex,
            transX,
            transY,
            transZ,
            mode,
            interpolationDuration,
            ticFrac,
            out translationOverrideSampleTime,
            out shouldCalcBones);

    public static void SetModelBoneScalingNative(
        bool setBoneOffsetSharedReturnedModel,
        double modelTimer,
        int modelIndex,
        int boneIndex,
        double scaleX,
        double scaleY,
        double scaleZ,
        int mode,
        double interpolationDuration,
        double ticFrac,
        out double scalingOverrideSampleTime,
        out bool shouldCalcBones)
    {
        if (!TryApplyBoneOverride(setBoneOffsetSharedReturnedModel, modelTimer, ticFrac, out scalingOverrideSampleTime, out shouldCalcBones))
            return;
        _ = (modelIndex, boneIndex, scaleX, scaleY, scaleZ, mode, interpolationDuration);
    }

    public static void SetBoneScalingNative(
        bool setBoneOffsetSharedReturnedModel,
        double modelTimer,
        int boneIndex,
        double scaleX,
        double scaleY,
        double scaleZ,
        int mode,
        double interpolationDuration,
        double ticFrac,
        out double scalingOverrideSampleTime,
        out bool shouldCalcBones) =>
        SetModelBoneScalingNative(
            setBoneOffsetSharedReturnedModel,
            modelTimer,
            DefaultModelIndex,
            boneIndex,
            scaleX,
            scaleY,
            scaleZ,
            mode,
            interpolationDuration,
            ticFrac,
            out scalingOverrideSampleTime,
            out shouldCalcBones);

    public static void SetModelNamedBoneScalingNative(
        bool setBoneOffsetSharedReturnedModel,
        double modelTimer,
        int modelIndex,
        int boneNameIndex,
        int resolvedBoneIndex,
        double scaleX,
        double scaleY,
        double scaleZ,
        int mode,
        double interpolationDuration,
        double ticFrac,
        out double scalingOverrideSampleTime,
        out bool shouldCalcBones)
    {
        _ = boneNameIndex;
        SetModelBoneScalingNative(
            setBoneOffsetSharedReturnedModel,
            modelTimer,
            modelIndex,
            resolvedBoneIndex,
            scaleX,
            scaleY,
            scaleZ,
            mode,
            interpolationDuration,
            ticFrac,
            out scalingOverrideSampleTime,
            out shouldCalcBones);
    }

    public static void SetNamedBoneScalingNative(
        bool setBoneOffsetSharedReturnedModel,
        double modelTimer,
        int boneNameIndex,
        int resolvedBoneIndex,
        double scaleX,
        double scaleY,
        double scaleZ,
        int mode,
        double interpolationDuration,
        double ticFrac,
        out double scalingOverrideSampleTime,
        out bool shouldCalcBones) =>
        SetModelNamedBoneScalingNative(
            setBoneOffsetSharedReturnedModel,
            modelTimer,
            DefaultModelIndex,
            boneNameIndex,
            resolvedBoneIndex,
            scaleX,
            scaleY,
            scaleZ,
            mode,
            interpolationDuration,
            ticFrac,
            out scalingOverrideSampleTime,
            out shouldCalcBones);

    public static bool ClearBoneOffsetsNative(bool hasModelData) => hasModelData;

    public static void GetBoneOffsetNative(
        bool getBoneOffsetSharedReturnedModel,
        int boneIndex,
        int numret,
        double modelTimer,
        bool hasOverrideMod,
        double translationX,
        double translationY,
        double translationZ,
        double rotationX,
        double rotationY,
        double rotationZ,
        double rotationW,
        double scalingX,
        double scalingY,
        double scalingZ,
        out int finalNumret,
        out double outTranslationX,
        out double outTranslationY,
        out double outTranslationZ,
        out double outRotationX,
        out double outRotationY,
        out double outRotationZ,
        out double outRotationW,
        out double outScalingX,
        out double outScalingY,
        out double outScalingZ)
    {
        outTranslationX = outTranslationY = outTranslationZ = 0;
        outRotationX = outRotationY = outRotationZ = 0;
        outRotationW = 1;
        outScalingX = outScalingY = outScalingZ = 0;
        if (getBoneOffsetSharedReturnedModel && hasOverrideMod)
        {
            _ = (GetBoneOffsetSampleTics(modelTimer), boneIndex);
            outTranslationX = translationX;
            outTranslationY = translationY;
            outTranslationZ = translationZ;
            outRotationX = rotationX;
            outRotationY = rotationY;
            outRotationZ = rotationZ;
            outRotationW = rotationW;
            outScalingX = scalingX;
            outScalingY = scalingY;
            outScalingZ = scalingZ;
        }

        finalNumret = FinalizeGetBoneOffsetNumret(numret);
    }

    public static void GetNamedBoneOffsetNative(
        bool getBoneOffsetSharedReturnedModel,
        int resolvedBoneIndex,
        int numret,
        double modelTimer,
        bool hasOverrideMod,
        double translationX,
        double translationY,
        double translationZ,
        double rotationX,
        double rotationY,
        double rotationZ,
        double rotationW,
        double scalingX,
        double scalingY,
        double scalingZ,
        out int finalNumret,
        out double outTranslationX,
        out double outTranslationY,
        out double outTranslationZ,
        out double outRotationX,
        out double outRotationY,
        out double outRotationZ,
        out double outRotationW,
        out double outScalingX,
        out double outScalingY,
        out double outScalingZ) =>
        GetBoneOffsetNative(
            getBoneOffsetSharedReturnedModel,
            resolvedBoneIndex,
            numret,
            modelTimer,
            hasOverrideMod,
            translationX,
            translationY,
            translationZ,
            rotationX,
            rotationY,
            rotationZ,
            rotationW,
            scalingX,
            scalingY,
            scalingZ,
            out finalNumret,
            out outTranslationX,
            out outTranslationY,
            out outTranslationZ,
            out outRotationX,
            out outRotationY,
            out outRotationZ,
            out outRotationW,
            out outScalingX,
            out outScalingY,
            out outScalingZ);
}

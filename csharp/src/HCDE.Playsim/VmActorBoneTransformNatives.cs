namespace HCDE.Playsim;

/// <summary>Bone rotation/translation/scaling/clear/get-offset natives from <c>p_actionfunctions.cpp</c> (5326–5647).</summary>
public static class VmActorBoneTransformNatives
{
    public const int DefaultModelIndex = 0;

    /// <summary>DEFINE_ACTION wrappers pass <c>1.0</c> as <c>ticFrac</c> (e.g. SetBoneRotation 5373).</summary>
    public const double ActionWrapperTicFrac = 1.0;

    public const double GetBoneOffsetTicOffset = 1.0;

    public static double ModelTimerPlusTicFrac(double modelTimer, double ticFrac) => modelTimer + ticFrac;

    public static int GetBoneOffsetSampleTics(double modelTimer) =>
        (int)(modelTimer + GetBoneOffsetTicOffset);

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
        if (!setBoneOffsetSharedReturnedModel)
        {
            rotationOverrideSampleTime = 0;
            shouldCalcBones = false;
            return;
        }

        rotationOverrideSampleTime = ModelTimerPlusTicFrac(modelTimer, ticFrac);
        BoneOverrideRotationSetSeam(modelIndex, boneIndex, rotX, rotY, rotZ, rotW, rotationOverrideSampleTime, interpolationDuration, mode);
        shouldCalcBones = true;
    }

    private static void BoneOverrideRotationSetSeam(
        int modelIndex,
        int boneIndex,
        double rotX,
        double rotY,
        double rotZ,
        double rotW,
        double sampleTime,
        double interpolationDuration,
        int mode) =>
        _ = (modelIndex, boneIndex, rotX, rotY, rotZ, rotW, sampleTime, interpolationDuration, mode);

    private static void BoneOverrideTranslationSetSeam(
        int modelIndex,
        int boneIndex,
        double transX,
        double transY,
        double transZ,
        double sampleTime,
        double interpolationDuration,
        int mode) =>
        _ = (modelIndex, boneIndex, transX, transY, transZ, sampleTime, interpolationDuration, mode);

    private static void BoneOverrideScalingSetSeam(
        int modelIndex,
        int boneIndex,
        double scaleX,
        double scaleY,
        double scaleZ,
        double sampleTime,
        double interpolationDuration,
        int mode) =>
        _ = (modelIndex, boneIndex, scaleX, scaleY, scaleZ, sampleTime, interpolationDuration, mode);

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
        if (!setBoneOffsetSharedReturnedModel)
        {
            rotationOverrideSampleTime = 0;
            shouldCalcBones = false;
            return;
        }

        rotationOverrideSampleTime = ModelTimerPlusTicFrac(modelTimer, ticFrac);
        BoneOverrideRotationSetSeam(modelIndex, resolvedBoneIndex, rotX, rotY, rotZ, rotW, rotationOverrideSampleTime, interpolationDuration, mode);
        _ = boneNameIndex;
        shouldCalcBones = true;
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

    public static void SetBoneRotationActionWrapper(
        double modelTimer,
        int boneIndex,
        double rotX,
        double rotY,
        double rotZ,
        double rotW,
        int mode,
        double interpolationDuration,
        bool setBoneOffsetSharedReturnedModel,
        out double rotationOverrideSampleTime,
        out bool shouldCalcBones) =>
        SetBoneRotationNative(
            setBoneOffsetSharedReturnedModel,
            modelTimer,
            boneIndex,
            rotX,
            rotY,
            rotZ,
            rotW,
            mode,
            interpolationDuration,
            ActionWrapperTicFrac,
            out rotationOverrideSampleTime,
            out shouldCalcBones);

    public static void SetNamedBoneRotationActionWrapper(
        double modelTimer,
        int boneNameIndex,
        int resolvedBoneIndex,
        double rotX,
        double rotY,
        double rotZ,
        double rotW,
        int mode,
        double interpolationDuration,
        bool setBoneOffsetSharedReturnedModel,
        out double rotationOverrideSampleTime,
        out bool shouldCalcBones) =>
        SetNamedBoneRotationNative(
            setBoneOffsetSharedReturnedModel,
            modelTimer,
            boneNameIndex,
            resolvedBoneIndex,
            rotX,
            rotY,
            rotZ,
            rotW,
            mode,
            interpolationDuration,
            ActionWrapperTicFrac,
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
        if (!setBoneOffsetSharedReturnedModel)
        {
            translationOverrideSampleTime = 0;
            shouldCalcBones = false;
            return;
        }

        translationOverrideSampleTime = ModelTimerPlusTicFrac(modelTimer, ticFrac);
        BoneOverrideTranslationSetSeam(modelIndex, boneIndex, transX, transY, transZ, translationOverrideSampleTime, interpolationDuration, mode);
        shouldCalcBones = true;
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
        if (!setBoneOffsetSharedReturnedModel)
        {
            translationOverrideSampleTime = 0;
            shouldCalcBones = false;
            return;
        }

        translationOverrideSampleTime = ModelTimerPlusTicFrac(modelTimer, ticFrac);
        BoneOverrideTranslationSetSeam(modelIndex, resolvedBoneIndex, transX, transY, transZ, translationOverrideSampleTime, interpolationDuration, mode);
        _ = boneNameIndex;
        shouldCalcBones = true;
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

    public static void SetBoneTranslationActionWrapper(
        double modelTimer,
        int boneIndex,
        double transX,
        double transY,
        double transZ,
        int mode,
        double interpolationDuration,
        bool setBoneOffsetSharedReturnedModel,
        out double translationOverrideSampleTime,
        out bool shouldCalcBones) =>
        SetBoneTranslationNative(
            setBoneOffsetSharedReturnedModel,
            modelTimer,
            boneIndex,
            transX,
            transY,
            transZ,
            mode,
            interpolationDuration,
            ActionWrapperTicFrac,
            out translationOverrideSampleTime,
            out shouldCalcBones);

    public static void SetNamedBoneTranslationActionWrapper(
        double modelTimer,
        int boneNameIndex,
        int resolvedBoneIndex,
        double transX,
        double transY,
        double transZ,
        int mode,
        double interpolationDuration,
        bool setBoneOffsetSharedReturnedModel,
        out double translationOverrideSampleTime,
        out bool shouldCalcBones) =>
        SetNamedBoneTranslationNative(
            setBoneOffsetSharedReturnedModel,
            modelTimer,
            boneNameIndex,
            resolvedBoneIndex,
            transX,
            transY,
            transZ,
            mode,
            interpolationDuration,
            ActionWrapperTicFrac,
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
        if (!setBoneOffsetSharedReturnedModel)
        {
            scalingOverrideSampleTime = 0;
            shouldCalcBones = false;
            return;
        }

        scalingOverrideSampleTime = ModelTimerPlusTicFrac(modelTimer, ticFrac);
        BoneOverrideScalingSetSeam(modelIndex, boneIndex, scaleX, scaleY, scaleZ, scalingOverrideSampleTime, interpolationDuration, mode);
        shouldCalcBones = true;
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
        if (!setBoneOffsetSharedReturnedModel)
        {
            scalingOverrideSampleTime = 0;
            shouldCalcBones = false;
            return;
        }

        scalingOverrideSampleTime = ModelTimerPlusTicFrac(modelTimer, ticFrac);
        BoneOverrideScalingSetSeam(modelIndex, resolvedBoneIndex, scaleX, scaleY, scaleZ, scalingOverrideSampleTime, interpolationDuration, mode);
        _ = boneNameIndex;
        shouldCalcBones = true;
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

    public static void SetBoneScalingActionWrapper(
        double modelTimer,
        int boneIndex,
        double scaleX,
        double scaleY,
        double scaleZ,
        int mode,
        double interpolationDuration,
        bool setBoneOffsetSharedReturnedModel,
        out double scalingOverrideSampleTime,
        out bool shouldCalcBones) =>
        SetBoneScalingNative(
            setBoneOffsetSharedReturnedModel,
            modelTimer,
            boneIndex,
            scaleX,
            scaleY,
            scaleZ,
            mode,
            interpolationDuration,
            ActionWrapperTicFrac,
            out scalingOverrideSampleTime,
            out shouldCalcBones);

    public static void SetNamedBoneScalingActionWrapper(
        double modelTimer,
        int boneNameIndex,
        int resolvedBoneIndex,
        double scaleX,
        double scaleY,
        double scaleZ,
        int mode,
        double interpolationDuration,
        bool setBoneOffsetSharedReturnedModel,
        out double scalingOverrideSampleTime,
        out bool shouldCalcBones) =>
        SetNamedBoneScalingNative(
            setBoneOffsetSharedReturnedModel,
            modelTimer,
            boneNameIndex,
            resolvedBoneIndex,
            scaleX,
            scaleY,
            scaleZ,
            mode,
            interpolationDuration,
            ActionWrapperTicFrac,
            out scalingOverrideSampleTime,
            out shouldCalcBones);

    public static bool ClearBoneOffsetsNative(bool hasModelData)
    {
        if (!hasModelData)
            return false;
        return true;
    }

    public static void ClearBoneOffsetsActionWrapper(bool hasModelData) => ClearBoneOffsetsNative(hasModelData);

    public static int FinalizeGetBoneOffsetNumret(int numret) => numret > 2 ? 3 : numret;

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
        outTranslationX = 0;
        outTranslationY = 0;
        outTranslationZ = 0;
        outRotationX = 0;
        outRotationY = 0;
        outRotationZ = 0;
        outRotationW = 1;
        outScalingX = 0;
        outScalingY = 0;
        outScalingZ = 0;

        if (getBoneOffsetSharedReturnedModel && hasOverrideMod)
        {
            _ = GetBoneOffsetSampleTics(modelTimer);
            _ = boneIndex;
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

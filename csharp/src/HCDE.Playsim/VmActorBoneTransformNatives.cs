namespace HCDE.Playsim;

/// <summary>Bone rotation/translation/scaling/clear/get-offset natives from <c>p_actionfunctions.cpp</c> (5326–5647).</summary>
public static class VmActorBoneTransformNatives
{
    public const int DefaultModelIndex = 0;
    public const double GetBoneOffsetTicOffset = 1.0;

    public static (double X, double Y, double Z, double W) DefaultBoneRotationVector() => (0, 0, 0, 1);

    public static (double X, double Y, double Z) DefaultBoneTranslationVector() => (0, 0, 0);

    public static (double X, double Y, double Z) DefaultBoneScalingVector() => (0, 0, 0);

    public static double BoneOverrideSampleTime(double modelTimer, double ticFrac) => modelTimer + ticFrac;

    public static int GetBoneOffsetSampleTics(double modelTimer) => (int)(modelTimer + GetBoneOffsetTicOffset);

    public static bool SetModelBoneRotationNative(
        bool setBoneOffsetReturnedModel,
        double modelTimer,
        double ticFrac,
        double interpolationDuration,
        int mode,
        double rotX,
        double rotY,
        double rotZ,
        double rotW,
        out double sampleTime,
        out bool shouldCalcBones)
    {
        sampleTime = BoneOverrideSampleTime(modelTimer, ticFrac);
        if (!setBoneOffsetReturnedModel)
        {
            shouldCalcBones = false;
            return false;
        }

        shouldCalcBones = true;
        return rotX != 0 || rotY != 0 || rotZ != 0 || rotW != 0 || mode != 0 || interpolationDuration != 0;
    }

    public static bool SetBoneRotationNative(
        bool setBoneOffsetReturnedModel,
        double modelTimer,
        double ticFrac,
        double interpolationDuration,
        int mode,
        double rotX,
        double rotY,
        double rotZ,
        double rotW,
        out double sampleTime,
        out bool shouldCalcBones) =>
        SetModelBoneRotationNative(
            setBoneOffsetReturnedModel,
            modelTimer,
            ticFrac,
            interpolationDuration,
            mode,
            rotX,
            rotY,
            rotZ,
            rotW,
            out sampleTime,
            out shouldCalcBones);

    public static bool SetModelNamedBoneRotationNative(
        bool setBoneOffsetReturnedModel,
        double modelTimer,
        double ticFrac,
        double interpolationDuration,
        int mode,
        double rotX,
        double rotY,
        double rotZ,
        double rotW,
        out double sampleTime,
        out bool shouldCalcBones) =>
        SetModelBoneRotationNative(
            setBoneOffsetReturnedModel,
            modelTimer,
            ticFrac,
            interpolationDuration,
            mode,
            rotX,
            rotY,
            rotZ,
            rotW,
            out sampleTime,
            out shouldCalcBones);

    public static bool SetNamedBoneRotationNative(
        int boneNameIndex,
        bool setBoneOffsetReturnedModel,
        double modelTimer,
        double ticFrac,
        double interpolationDuration,
        int mode,
        double rotX,
        double rotY,
        double rotZ,
        double rotW,
        out double sampleTime,
        out bool shouldCalcBones) =>
        SetModelNamedBoneRotationNative(
            setBoneOffsetReturnedModel,
            modelTimer,
            ticFrac,
            interpolationDuration,
            mode,
            rotX,
            rotY,
            rotZ,
            rotW,
            out sampleTime,
            out shouldCalcBones);

    public static bool SetModelBoneTranslationNative(
        bool setBoneOffsetReturnedModel,
        double modelTimer,
        double ticFrac,
        double interpolationDuration,
        int mode,
        double x,
        double y,
        double z,
        out double sampleTime,
        out bool shouldCalcBones)
    {
        sampleTime = BoneOverrideSampleTime(modelTimer, ticFrac);
        if (!setBoneOffsetReturnedModel)
        {
            shouldCalcBones = false;
            return false;
        }

        shouldCalcBones = true;
        return x != 0 || y != 0 || z != 0 || mode != 0 || interpolationDuration != 0;
    }

    public static bool SetBoneTranslationNative(
        bool setBoneOffsetReturnedModel,
        double modelTimer,
        double ticFrac,
        double interpolationDuration,
        int mode,
        double x,
        double y,
        double z,
        out double sampleTime,
        out bool shouldCalcBones) =>
        SetModelBoneTranslationNative(
            setBoneOffsetReturnedModel,
            modelTimer,
            ticFrac,
            interpolationDuration,
            mode,
            x,
            y,
            z,
            out sampleTime,
            out shouldCalcBones);

    public static bool SetModelNamedBoneTranslationNative(
        bool setBoneOffsetReturnedModel,
        double modelTimer,
        double ticFrac,
        double interpolationDuration,
        int mode,
        double x,
        double y,
        double z,
        out double sampleTime,
        out bool shouldCalcBones) =>
        SetModelBoneTranslationNative(
            setBoneOffsetReturnedModel,
            modelTimer,
            ticFrac,
            interpolationDuration,
            mode,
            x,
            y,
            z,
            out sampleTime,
            out shouldCalcBones);

    public static bool SetNamedBoneTranslationNative(
        bool setBoneOffsetReturnedModel,
        double modelTimer,
        double ticFrac,
        double interpolationDuration,
        int mode,
        double x,
        double y,
        double z,
        out double sampleTime,
        out bool shouldCalcBones) =>
        SetModelNamedBoneTranslationNative(
            setBoneOffsetReturnedModel,
            modelTimer,
            ticFrac,
            interpolationDuration,
            mode,
            x,
            y,
            z,
            out sampleTime,
            out shouldCalcBones);

    public static bool SetModelBoneScalingNative(
        bool setBoneOffsetReturnedModel,
        double modelTimer,
        double ticFrac,
        double interpolationDuration,
        int mode,
        double x,
        double y,
        double z,
        out double sampleTime,
        out bool shouldCalcBones)
    {
        sampleTime = BoneOverrideSampleTime(modelTimer, ticFrac);
        if (!setBoneOffsetReturnedModel)
        {
            shouldCalcBones = false;
            return false;
        }

        shouldCalcBones = true;
        return x != 0 || y != 0 || z != 0 || mode != 0 || interpolationDuration != 0;
    }

    public static bool SetBoneScalingNative(
        bool setBoneOffsetReturnedModel,
        double modelTimer,
        double ticFrac,
        double interpolationDuration,
        int mode,
        double x,
        double y,
        double z,
        out double sampleTime,
        out bool shouldCalcBones) =>
        SetModelBoneScalingNative(
            setBoneOffsetReturnedModel,
            modelTimer,
            ticFrac,
            interpolationDuration,
            mode,
            x,
            y,
            z,
            out sampleTime,
            out shouldCalcBones);

    public static bool SetModelNamedBoneScalingNative(
        bool setBoneOffsetReturnedModel,
        double modelTimer,
        double ticFrac,
        double interpolationDuration,
        int mode,
        double x,
        double y,
        double z,
        out double sampleTime,
        out bool shouldCalcBones) =>
        SetModelBoneScalingNative(
            setBoneOffsetReturnedModel,
            modelTimer,
            ticFrac,
            interpolationDuration,
            mode,
            x,
            y,
            z,
            out sampleTime,
            out shouldCalcBones);

    public static bool SetNamedBoneScalingNative(
        bool setBoneOffsetReturnedModel,
        double modelTimer,
        double ticFrac,
        double interpolationDuration,
        int mode,
        double x,
        double y,
        double z,
        out double sampleTime,
        out bool shouldCalcBones) =>
        SetModelNamedBoneScalingNative(
            setBoneOffsetReturnedModel,
            modelTimer,
            ticFrac,
            interpolationDuration,
            mode,
            x,
            y,
            z,
            out sampleTime,
            out shouldCalcBones);

    public static bool ClearBoneOffsetsNative(bool hasModelData) => hasModelData;

    public static int FinalizeGetBoneOffsetNumret(int numret)
    {
        if (numret > 2)
            return 3;
        return numret;
    }

    public static bool GetBoneOffsetAssignsScaling(int numret) => numret > 2;

    public static bool GetBoneOffsetAssignsTranslation(int numret) => numret > 1;

    public static bool GetBoneOffsetAssignsRotation(int numret) => numret > 0;

    public static bool GetBoneOffsetNative(
        bool getBoneOffsetReturnedModel,
        int numret,
        out int finalNumret,
        out bool useDefaultsOnly)
    {
        useDefaultsOnly = !getBoneOffsetReturnedModel;
        finalNumret = FinalizeGetBoneOffsetNumret(numret);
        return true;
    }

    public static bool GetNamedBoneOffsetNative(
        bool getBoneOffsetReturnedModel,
        int numret,
        out int finalNumret,
        out bool useDefaultsOnly) =>
        GetBoneOffsetNative(getBoneOffsetReturnedModel, numret, out finalNumret, out useDefaultsOnly);
}

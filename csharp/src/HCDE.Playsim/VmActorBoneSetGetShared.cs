namespace HCDE.Playsim;

/// <summary>Bone offset/get/set helpers from <c>p_actionfunctions.cpp</c> (5272–5464).</summary>
public static class VmActorBoneSetGetShared
{
    public const int MaxSetBoneMode = 2;

    public enum BoneAccessKind
    {
        GetBone,
        GetBoneOffset,
        SetBoneOffset,
    }

    public readonly record struct BoneOverrideResizePlan(int ModelIndex, int JointCount, bool NeedsModelSlotGrow, bool NeedsJointGrow);

    public static bool RequiresDecoupledAnimations(bool decoupled) => decoupled;

    public static string DecoupledAnimationsError(BoneAccessKind kind, bool isSet) => kind switch
    {
        BoneAccessKind.SetBoneOffset => "Cannot set bone offset for non-decoupled actors",
        BoneAccessKind.GetBoneOffset => "Cannot get bone for non-decoupled actors",
        _ => isSet ? "Cannot set bone offset for non-decoupled actors" : "Cannot get bone offset for non-decoupled actors",
    };

    public static double NormalizeInterpolationDuration(double duration) => duration < 0 ? 0 : duration;

    public static bool IsValidSetBoneMode(int mode) => mode >= 0 && mode <= MaxSetBoneMode;

    public static bool TryValidateBoneIndex(int boneIndex, int jointCount, out string? error)
    {
        if (boneIndex < 0 || boneIndex >= jointCount)
        {
            error = "bone index out of range";
            return false;
        }

        error = null;
        return true;
    }

    public static bool TryResolveNamedBoneIndex(int foundIndex, int jointCount, out string? notifyMessage)
    {
        if (foundIndex < 0 || foundIndex >= jointCount)
        {
            notifyMessage = "Could not find bone";
            return false;
        }

        notifyMessage = null;
        return true;
    }

    public static BoneOverrideResizePlan PlanBoneOverrideResize(int modelBoneOverrideSize, int modelIndex, int jointCount)
    {
        var needsModel = modelBoneOverrideSize <= modelIndex;
        return new BoneOverrideResizePlan(modelIndex, jointCount, needsModel, true);
    }

    public static int BoneOverrideTargetSize(int modelIndex) => modelIndex + 1;

    public static bool ShouldAbortBoneNative(FModelResolution resolution) => resolution == FModelResolution.Abort;

    public static bool ShouldSkipBoneTransform(FModelResolution resolution) =>
        resolution is FModelResolution.MissingNamedBone or FModelResolution.Abort;

    public enum FModelResolution
    {
        Ok,
        MissingNamedBone,
        Abort,
    }

    public static FModelResolution PlanSetGetBoneSharedIndex(
        bool decoupledAnimations,
        bool hasModelDefOrClass,
        bool modelIndexInRuntime,
        bool modelIndexInBaseFrames,
        bool hasNamedBone,
        int namedBoneIndex,
        int boneIndex,
        int jointCount)
    {
        if (!decoupledAnimations)
            return FModelResolution.Abort;
        if (!hasModelDefOrClass)
            return FModelResolution.Abort;
        if (!modelIndexInRuntime && !modelIndexInBaseFrames)
            return FModelResolution.Abort;

        if (hasNamedBone)
        {
            return TryResolveNamedBoneIndex(namedBoneIndex, jointCount, out _)
                ? FModelResolution.Ok
                : FModelResolution.MissingNamedBone;
        }

        return TryValidateBoneIndex(boneIndex, jointCount, out _)
            ? FModelResolution.Ok
            : FModelResolution.Abort;
    }

    public static (FModelResolution Resolution, double Duration) PlanSetBoneOffsetShared(
        bool decoupledAnimations,
        double interpolationDuration,
        int mode,
        bool hasModelDefOrClass,
        bool modelResolvable,
        int boneIndex,
        int jointCount)
    {
        var duration = NormalizeInterpolationDuration(interpolationDuration);
        if (!IsValidSetBoneMode(mode))
            return (FModelResolution.Abort, duration);
        if (!decoupledAnimations || !hasModelDefOrClass || !modelResolvable)
            return (FModelResolution.Abort, duration);
        if (!TryValidateBoneIndex(boneIndex, jointCount, out _))
            return (FModelResolution.Abort, duration);
        return (FModelResolution.Ok, duration);
    }

    public static FModelResolution PlanGetBoneOffsetShared(
        bool decoupledAnimations,
        bool hasModelDefOrClass,
        bool modelResolvable,
        bool hasNamedBone,
        int namedBoneIndex,
        int boneIndex,
        int jointCount) =>
        PlanSetGetBoneSharedIndex(
            decoupledAnimations,
            hasModelDefOrClass,
            modelResolvable,
            modelResolvable,
            hasNamedBone,
            namedBoneIndex,
            boneIndex,
            jointCount);

    public static bool NamedBoneRotationUsesModelIndexZero(bool useDefaultModel) => useDefaultModel;

    public static int DefaultModelIndexForActorBone() => 0;

    public static bool BoneRotationNativeShouldCalcBones(FModelResolution resolution) => resolution == FModelResolution.Ok;

    public static bool BoneTranslationNativeShouldCalcBones(FModelResolution resolution) => resolution == FModelResolution.Ok;

    public static (double X, double Y, double Z, double W) PackBoneRotation(double rotX, double rotY, double rotZ, double rotW) =>
        (rotX, rotY, rotZ, rotW);

    public static (double X, double Y, double Z) PackBoneTranslation(double x, double y, double z) => (x, y, z);

    public static bool ActionNativeForwardsTicFrac(double ticFrac) => ticFrac > 0;
}

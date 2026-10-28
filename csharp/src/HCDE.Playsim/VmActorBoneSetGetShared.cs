namespace HCDE.Playsim;

/// <summary>Bone offset/get/set helpers from <c>p_actionfunctions.cpp</c> (5272–5464).</summary>
public static class VmActorBoneSetGetShared
{
    public const int MaxSetBoneMode = 2;
    public static double NormalizeInterpolationDuration(double duration) => duration < 0 ? 0 : duration;

    public static bool IsValidSetBoneMode(int mode) => mode >= 0 && mode <= MaxSetBoneMode;

    public static bool NeedsBoneOverrideModelSlotGrow(int modelBoneOverrideSize, int modelIndex) =>
        modelBoneOverrideSize <= modelIndex;

    public static int BoneOverrideModelSlotCount(int modelIndex) => modelIndex + 1;

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

    public static bool SetGetBoneSharedIndexGate(
        bool decoupledAnimations,
        bool hasModelDefOrClass,
        bool modelIndexInRuntime,
        bool modelIndexInBaseFrames,
        bool hasNamedBone,
        int namedBoneIndex,
        int boneIndex,
        int jointCount,
        out bool missingNamedBone)
    {
        missingNamedBone = false;
        if (!decoupledAnimations || !hasModelDefOrClass || (!modelIndexInRuntime && !modelIndexInBaseFrames))
            return false;

        if (hasNamedBone)
        {
            if (!TryResolveNamedBoneIndex(namedBoneIndex, jointCount, out _))
            {
                missingNamedBone = true;
                return false;
            }

            return true;
        }

        return TryValidateBoneIndex(boneIndex, jointCount, out _);
    }

    public static bool TryPrepareSetBoneOffset(
        double interpolationDuration,
        int mode,
        int boneIndex,
        int jointCount,
        out double clampedDuration)
    {
        clampedDuration = NormalizeInterpolationDuration(interpolationDuration);
        if (!IsValidSetBoneMode(mode))
            return false;
        return TryValidateBoneIndex(boneIndex, jointCount, out _);
    }

}

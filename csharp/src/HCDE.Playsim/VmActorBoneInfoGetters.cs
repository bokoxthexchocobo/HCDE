namespace HCDE.Playsim;

/// <summary>Bone info getter natives from <c>p_actionfunctions.cpp</c> (5656–5882).</summary>
public static class VmActorBoneInfoGetters
{
    public readonly record struct BoneTrsVectors(
        double TranslationX,
        double TranslationY,
        double TranslationZ,
        double RotationX,
        double RotationY,
        double RotationZ,
        double RotationW,
        double ScalingX,
        double ScalingY,
        double ScalingZ);

    public static BoneTrsVectors DefaultBoneTrsVectors() =>
        new(0, 0, 0, 0, 0, 0, 1, 0, 0, 0);

    public static bool ShouldFillBoneArray(bool outPointerNonNull, bool boneSharedOk) =>
        outPointerNonNull && boneSharedOk;

    public static void GetRootBonesNative(bool outPointerNonNull, bool setGetBoneSharedOk)
    {
        if (!ShouldFillBoneArray(outPointerNonNull, setGetBoneSharedOk))
            return;
        // mdl->GetRootJoints(*out);
    }

    public static void GetRootBonesActionWrapper(bool outPointerNonNull, bool setGetBoneSharedOk) =>
        GetRootBonesNative(outPointerNonNull, setGetBoneSharedOk);

    public static int GetBoneNameNative(int jointNameIndexFromModel) => jointNameIndexFromModel;

    public static int GetBoneIndexNative(int resolvedBoneIndex) => resolvedBoneIndex;

    public static int GetBoneParentNative(int jointParentFromModel) => jointParentFromModel;

    public static int GetNamedBoneParentNative(int jointParentFromModel) => jointParentFromModel;

    public static void GetBoneChildrenNative(bool outPointerNonNull, bool getBoneSharedOk, int boneIndex)
    {
        if (!ShouldFillBoneArray(outPointerNonNull, getBoneSharedOk))
            return;
        // mdl->GetJointChildren(bone_index, *out);
        _ = boneIndex;
    }

    public static void GetNamedBoneChildrenNative(bool outPointerNonNull, bool getBoneSharedOk, int resolvedBoneIndex)
    {
        if (!outPointerNonNull)
            return;
        if (!getBoneSharedOk)
            return;
        // FName bone_name {ENamedName(boneName_i)}; GetBoneShared(..., &bone_name); GetJointChildren
        _ = resolvedBoneIndex;
    }

    public static BoneTrsVectors SelectBoneTrsVectors(
        bool getBoneSharedOk,
        bool hasPoseFromModel,
        double poseTx,
        double poseTy,
        double poseTz,
        double poseRx,
        double poseRy,
        double poseRz,
        double poseRw,
        double poseSx,
        double poseSy,
        double poseSz)
    {
        if (!getBoneSharedOk || !hasPoseFromModel)
            return DefaultBoneTrsVectors();

        return new BoneTrsVectors(
            poseTx,
            poseTy,
            poseTz,
            poseRx,
            poseRy,
            poseRz,
            poseRw,
            poseSx,
            poseSy,
            poseSz);
    }

    public static int EmitBoneTrsReturns(int numret, BoneTrsVectors trs)
    {
        if (numret > 2)
        {
            _ = (trs.ScalingX, trs.ScalingY, trs.ScalingZ);
            numret = 3;
        }

        if (numret > 1)
            _ = (trs.TranslationX, trs.TranslationY, trs.TranslationZ);

        if (numret > 0)
            _ = (trs.RotationX, trs.RotationY, trs.RotationZ, trs.RotationW);

        return numret;
    }

    public static int GetBoneBaseTrsNative(
        bool getBoneSharedOk,
        int boneIndex,
        int numret,
        bool hasPoseFromModel,
        double poseTx,
        double poseTy,
        double poseTz,
        double poseRx,
        double poseRy,
        double poseRz,
        double poseRw,
        double poseSx,
        double poseSy,
        double poseSz)
    {
        var trs = SelectBoneTrsVectors(
            getBoneSharedOk,
            hasPoseFromModel,
            poseTx,
            poseTy,
            poseTz,
            poseRx,
            poseRy,
            poseRz,
            poseRw,
            poseSx,
            poseSy,
            poseSz);

        _ = boneIndex;
        return EmitBoneTrsReturns(numret, trs);
    }

    public static int GetNamedBoneBaseTrsNative(
        bool getBoneSharedOk,
        int resolvedBoneIndex,
        int numret,
        bool hasPoseFromModel,
        double poseTx,
        double poseTy,
        double poseTz,
        double poseRx,
        double poseRy,
        double poseRz,
        double poseRw,
        double poseSx,
        double poseSy,
        double poseSz)
    {
        // GetBoneShared(self, 0, bone_index, &bone_name) then same TRS fill as indexed path.
        return GetBoneBaseTrsNative(
            getBoneSharedOk,
            resolvedBoneIndex,
            numret,
            hasPoseFromModel,
            poseTx,
            poseTy,
            poseTz,
            poseRx,
            poseRy,
            poseRz,
            poseRw,
            poseSx,
            poseSy,
            poseSz);
    }

    public static int GetBoneCountNative(int jointCountFromModel) => jointCountFromModel;

    public static int GetBoneCountActionWrapper(int jointCountFromModel) =>
        GetBoneCountNative(jointCountFromModel);
}

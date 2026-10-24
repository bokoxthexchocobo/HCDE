namespace HCDE.Playsim;

/// <summary>Bone info getter natives from <c>p_actionfunctions.cpp</c> (5656–5882).</summary>
public static class VmActorBoneInfoGetters
{
    public static void GetRootBonesNative(bool outPointerNonNull, bool setGetBoneSharedOk)
    {
        if (!outPointerNonNull)
            return;
        if (!setGetBoneSharedOk)
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
        if (!outPointerNonNull)
            return;
        if (!getBoneSharedOk)
            return;
        // mdl->GetJointChildren(bone_index, *out);
        _ = boneIndex;
    }

    public static void GetNamedBoneChildrenNative(bool outPointerNonNull, bool getBoneSharedOk, int resolvedBoneIndex) =>
        GetBoneChildrenNative(outPointerNonNull, getBoneSharedOk, resolvedBoneIndex);

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
        double translationX = 0;
        double translationY = 0;
        double translationZ = 0;
        double rotationX = 0;
        double rotationY = 0;
        double rotationZ = 0;
        double rotationW = 1;
        double scalingX = 0;
        double scalingY = 0;
        double scalingZ = 0;

        if (getBoneSharedOk && hasPoseFromModel)
        {
            translationX = poseTx;
            translationY = poseTy;
            translationZ = poseTz;
            rotationX = poseRx;
            rotationY = poseRy;
            rotationZ = poseRz;
            rotationW = poseRw;
            scalingX = poseSx;
            scalingY = poseSy;
            scalingZ = poseSz;
        }

        if (numret > 2)
        {
            _ = (scalingX, scalingY, scalingZ);
            numret = 3;
        }

        if (numret > 1)
            _ = (translationX, translationY, translationZ);

        if (numret > 0)
            _ = (rotationX, rotationY, rotationZ, rotationW);

        _ = boneIndex;
        return numret;
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
        double poseSz) =>
        GetBoneBaseTrsNative(
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

    public static int GetBoneCountNative(int jointCountFromModel) => jointCountFromModel;

    public static int GetBoneCountActionWrapper(int jointCountFromModel) =>
        GetBoneCountNative(jointCountFromModel);
}

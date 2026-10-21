namespace HCDE.Playsim;

/// <summary>Bone info getter natives from <c>p_actionfunctions.cpp</c> (5656–5882).</summary>
public static class VmActorBoneInfoGetters
{
    public const int DefaultModelIndex = 0;

    public static bool GetRootBonesNative(bool outPointerNonNull, bool setGetBoneSharedOk, out bool shouldFillRootJoints)
    {
        shouldFillRootJoints = false;
        if (!outPointerNonNull)
            return false;
        if (!setGetBoneSharedOk)
            return false;
        shouldFillRootJoints = true;
        return true;
    }

    public static int GetBoneNameNative(bool getBoneSharedOk, int boneIndex, int jointNameIndexFromModel) =>
        getBoneSharedOk ? jointNameIndexFromModel : boneIndex;

    public static int GetBoneIndexNative(bool getBoneSharedOk, int resolvedBoneIndex) =>
        getBoneSharedOk ? resolvedBoneIndex : -1;

    public static int GetBoneParentNative(bool getBoneSharedOk, int jointParentFromModel) =>
        getBoneSharedOk ? jointParentFromModel : -1;

    public static int GetNamedBoneParentNative(bool getBoneSharedOk, int jointParentFromModel) =>
        GetBoneParentNative(getBoneSharedOk, jointParentFromModel);

    public static bool GetBoneChildrenNative(
        bool outPointerNonNull,
        bool getBoneSharedOk,
        int boneIndex,
        out bool shouldFillChildren)
    {
        shouldFillChildren = false;
        if (!outPointerNonNull)
            return false;
        if (!getBoneSharedOk)
            return false;
        shouldFillChildren = boneIndex >= 0;
        return shouldFillChildren;
    }

    public static bool GetNamedBoneChildrenNative(
        bool outPointerNonNull,
        bool getBoneSharedOk,
        int resolvedBoneIndex,
        out bool shouldFillChildren) =>
        GetBoneChildrenNative(outPointerNonNull, getBoneSharedOk, resolvedBoneIndex, out shouldFillChildren);

    public static (double Tx, double Ty, double Tz, double Rx, double Ry, double Rz, double Rw, double Sx, double Sy, double Sz)
        DefaultBoneBaseTrs() => (0, 0, 0, 0, 0, 0, 1, 0, 0, 0);

    public static (double Tx, double Ty, double Tz, double Rx, double Ry, double Rz, double Rw, double Sx, double Sy, double Sz)
        BoneBaseTrsFromPose(
            double tx,
            double ty,
            double tz,
            double rx,
            double ry,
            double rz,
            double rw,
            double sx,
            double sy,
            double sz) => (tx, ty, tz, rx, ry, rz, rw, sx, sy, sz);

    public static int FinalizeBoneBaseTrsNumret(int numret) => numret > 2 ? 3 : numret;

    public static bool GetBoneBaseTrsAssignsScaling(int numret) => numret > 2;

    public static bool GetBoneBaseTrsAssignsTranslation(int numret) => numret > 1;

    public static bool GetBoneBaseTrsAssignsRotation(int numret) => numret > 0;

    public static int ApplyGetBoneBaseTrsNumretAfterScaling(int numret) => numret > 2 ? 3 : numret;

    public static bool GetBoneBaseTrsNative(
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
        double poseSz,
        out int finalNumret,
        out double tx,
        out double ty,
        out double tz,
        out double rx,
        out double ry,
        out double rz,
        out double rw,
        out double sx,
        out double sy,
        out double sz)
    {
        var defaults = DefaultBoneBaseTrs();
        tx = defaults.Tx;
        ty = defaults.Ty;
        tz = defaults.Tz;
        rx = defaults.Rx;
        ry = defaults.Ry;
        rz = defaults.Rz;
        rw = defaults.Rw;
        sx = defaults.Sx;
        sy = defaults.Sy;
        sz = defaults.Sz;

        if (getBoneSharedOk && hasPoseFromModel)
        {
            var pose = BoneBaseTrsFromPose(
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
            tx = pose.Tx;
            ty = pose.Ty;
            tz = pose.Tz;
            rx = pose.Rx;
            ry = pose.Ry;
            rz = pose.Rz;
            rw = pose.Rw;
            sx = pose.Sx;
            sy = pose.Sy;
            sz = pose.Sz;
        }

        var workingNumret = numret;
        if (GetBoneBaseTrsAssignsScaling(workingNumret))
            workingNumret = ApplyGetBoneBaseTrsNumretAfterScaling(workingNumret);

        finalNumret = FinalizeBoneBaseTrsNumret(numret);
        if (GetBoneBaseTrsAssignsScaling(numret))
            finalNumret = ApplyGetBoneBaseTrsNumretAfterScaling(finalNumret);

        return boneIndex >= 0;
    }

    public static bool GetNamedBoneBaseTrsNative(
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
        double poseSz,
        out int finalNumret,
        out double tx,
        out double ty,
        out double tz,
        out double rx,
        out double ry,
        out double rz,
        out double rw,
        out double sx,
        out double sy,
        out double sz) =>
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
            poseSz,
            out finalNumret,
            out tx,
            out ty,
            out tz,
            out rx,
            out ry,
            out rz,
            out rw,
            out sx,
            out sy,
            out sz);

    public static int GetBoneCountNative(bool setGetBoneSharedOk, int jointCountFromModel) =>
        setGetBoneSharedOk ? jointCountFromModel : -1;
}

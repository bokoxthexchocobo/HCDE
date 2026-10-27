namespace HCDE.Playsim;

public static class VmActorBoneEulerTransform
{
    public static (double X, double Y, double Z) ZeroEulerAngles() => (0, 0, 0);

    public static bool ShouldReturnTransformPosition(int numret) => numret > 0;

    public static bool ShouldReturnTransformForward(int numret) => numret > 1;

    public static bool ShouldReturnTransformUp(int numret) => numret > 2;

    public static bool MatrixFillEligible(bool outMatrixNonNull, bool getBoneSharedOk) =>
        outMatrixNonNull && getBoneSharedOk;

    public static bool TransformUsesActorBonePose(bool getBoneSharedOk, bool hasBonePositionFromActor) =>
        getBoneSharedOk && hasBonePositionFromActor;

    public static (double X, double Y, double Z) GetBoneEulerAnglesNative(
        bool getBoneSharedOk,
        bool withOverride,
        double eulerX,
        double eulerY,
        double eulerZ) =>
        getBoneSharedOk ? (eulerX, eulerY, eulerZ) : ZeroEulerAngles();

    public static (double X, double Y, double Z) GetNamedBoneEulerAnglesNative(
        bool getBoneSharedOk,
        bool withOverride,
        double eulerX,
        double eulerY,
        double eulerZ) =>
        GetBoneEulerAnglesNative(getBoneSharedOk, withOverride, eulerX, eulerY, eulerZ);

    public static int TransformByBoneNative(
        bool getBoneSharedOk,
        bool withOverride,
        int boneIndex,
        int numret,
        double posX,
        double posY,
        double posZ,
        double fwdX,
        double fwdY,
        double fwdZ,
        double upX,
        double upY,
        double upZ,
        bool hasBonePositionFromActor,
        double outPosX,
        double outPosY,
        double outPosZ,
        double outFwdX,
        double outFwdY,
        double outFwdZ,
        double outUpX,
        double outUpY,
        double outUpZ)
    {
        double positionX = posX;
        double positionY = posY;
        double positionZ = posZ;
        double fwdx = fwdX;
        double fwdy = fwdY;
        double fwdz = fwdZ;
        double upx = upX;
        double upy = upY;
        double upz = upZ;

        if (TransformUsesActorBonePose(getBoneSharedOk, hasBonePositionFromActor))
        {
            positionX = outPosX;
            positionY = outPosY;
            positionZ = outPosZ;
            fwdx = outFwdX;
            fwdy = outFwdY;
            fwdz = outFwdZ;
            upx = outUpX;
            upy = outUpY;
            upz = outUpZ;
        }

        _ = withOverride;
        if (ShouldReturnTransformUp(numret))
            _ = (upx, upy, upz);
        if (ShouldReturnTransformForward(numret))
            _ = (fwdx, fwdy, fwdz);
        if (ShouldReturnTransformPosition(numret))
            _ = (positionX, positionY, positionZ);

        _ = boneIndex;
        return numret > 2 ? numret : numret;
    }

    public static int TransformByNamedBoneNative(
        bool getBoneSharedOk,
        bool withOverride,
        int resolvedBoneIndex,
        int numret,
        double posX,
        double posY,
        double posZ,
        double fwdX,
        double fwdY,
        double fwdZ,
        double upX,
        double upY,
        double upZ,
        bool hasBonePositionFromActor,
        double outPosX,
        double outPosY,
        double outPosZ,
        double outFwdX,
        double outFwdY,
        double outFwdZ,
        double outUpX,
        double outUpY,
        double outUpZ) =>
        TransformByBoneNative(
            getBoneSharedOk,
            withOverride,
            resolvedBoneIndex,
            numret,
            posX,
            posY,
            posZ,
            fwdX,
            fwdY,
            fwdZ,
            upX,
            upY,
            upZ,
            hasBonePositionFromActor,
            outPosX,
            outPosY,
            outPosZ,
            outFwdX,
            outFwdY,
            outFwdZ,
            outUpX,
            outUpY,
            outUpZ);

    public static void GetBoneMatrixRawNative(
        bool outMatrixNonNull,
        bool getBoneSharedOk,
        bool withOverride,
        int boneIndex,
        out bool shouldFillMatrix)
    {
        shouldFillMatrix = MatrixFillEligible(outMatrixNonNull, getBoneSharedOk);
        if (!shouldFillMatrix)
            return;
        _ = (withOverride, boneIndex);
    }

    public static void GetNamedBoneMatrixRawNative(
        bool outMatrixNonNull,
        bool getBoneSharedOk,
        bool withOverride,
        int resolvedBoneIndex,
        out bool shouldFillMatrix) =>
        GetBoneMatrixRawNative(outMatrixNonNull, getBoneSharedOk, withOverride, resolvedBoneIndex, out shouldFillMatrix);
}

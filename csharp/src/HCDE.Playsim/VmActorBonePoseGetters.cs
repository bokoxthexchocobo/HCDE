namespace HCDE.Playsim;

/// <summary>Bone pose / anim getters from <c>p_actionfunctions.cpp</c> (5890–6147).</summary>
public static class VmActorBonePoseGetters
{
    public static int GetAnimStartFrameNative(int firstFrameFromModel) => firstFrameFromModel;

    public static int GetAnimEndFrameNative(int lastFrameFromModel) => lastFrameFromModel;

    public static double GetAnimFramerateNative(double framerateFromModel) => framerateFromModel;

    public static int GetAnimStartFrameActionWrapper(int animNameIndex, int firstFrameFromModel) =>
        GetAnimStartFrameNative(firstFrameFromModel);

    public static int GetAnimEndFrameActionWrapper(int animNameIndex, int lastFrameFromModel) =>
        GetAnimEndFrameNative(lastFrameFromModel);

    public static double GetAnimFramerateActionWrapper(int animNameIndex, double framerateFromModel) =>
        GetAnimFramerateNative(framerateFromModel);

    public static int GetBoneFramePoseNative(
        bool getBoneSharedOk,
        int boneIndex,
        int frameIndex,
        int numFrames,
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

        if (getBoneSharedOk && frameIndex < numFrames && hasPoseFromModel)
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

    public static int GetNamedBoneFramePoseNative(
        bool getBoneSharedOk,
        int resolvedBoneIndex,
        int frameIndex,
        int numFrames,
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
        GetBoneFramePoseNative(
            getBoneSharedOk,
            resolvedBoneIndex,
            frameIndex,
            numFrames,
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

    public static (double X, double Y, double Z) GetBoneBasePositionNative(
        bool getBoneSharedOk,
        double posX,
        double posY,
        double posZ) =>
        getBoneSharedOk ? (posX, posY, posZ) : (0, 0, 0);

    public static (double X, double Y, double Z) GetNamedBoneBasePositionNative(
        bool getBoneSharedOk,
        double posX,
        double posY,
        double posZ) =>
        GetBoneBasePositionNative(getBoneSharedOk, posX, posY, posZ);

    public static (double X, double Y, double Z, double W) GetBoneBaseRotationNative(
        bool getBoneSharedOk,
        double rotX,
        double rotY,
        double rotZ,
        double rotW) =>
        getBoneSharedOk ? (rotX, rotY, rotZ, rotW) : (0, 0, 0, 1);

    public static (double X, double Y, double Z, double W) GetNamedBoneBaseRotationNative(
        bool getBoneSharedOk,
        double rotX,
        double rotY,
        double rotZ,
        double rotW) =>
        GetBoneBaseRotationNative(getBoneSharedOk, rotX, rotY, rotZ, rotW);

    public static int GetBoneTrsNative(
        bool getBoneSharedOk,
        bool withOverride,
        int boneIndex,
        int numret,
        bool hasTrsFromActor,
        double trsTx,
        double trsTy,
        double trsTz,
        double trsRx,
        double trsRy,
        double trsRz,
        double trsRw,
        double trsSx,
        double trsSy,
        double trsSz)
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

        if (getBoneSharedOk && hasTrsFromActor)
        {
            translationX = trsTx;
            translationY = trsTy;
            translationZ = trsTz;
            rotationX = trsRx;
            rotationY = trsRy;
            rotationZ = trsRz;
            rotationW = trsRw;
            scalingX = trsSx;
            scalingY = trsSy;
            scalingZ = trsSz;
        }

        _ = withOverride;
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

    public static int GetNamedBoneTrsNative(
        bool getBoneSharedOk,
        bool withOverride,
        int resolvedBoneIndex,
        int numret,
        bool hasTrsFromActor,
        double trsTx,
        double trsTy,
        double trsTz,
        double trsRx,
        double trsRy,
        double trsRz,
        double trsRw,
        double trsSx,
        double trsSy,
        double trsSz) =>
        GetBoneTrsNative(
            getBoneSharedOk,
            withOverride,
            resolvedBoneIndex,
            numret,
            hasTrsFromActor,
            trsTx,
            trsTy,
            trsTz,
            trsRx,
            trsRy,
            trsRz,
            trsRw,
            trsSx,
            trsSy,
            trsSz);
}

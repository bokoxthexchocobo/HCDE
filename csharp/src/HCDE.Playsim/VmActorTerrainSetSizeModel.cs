namespace HCDE.Playsim;

/// <summary><c>A_CheckTerrain</c>, <c>A_SetSize</c>, <c>SetCamera</c>, model-data helpers from <c>p_actionfunctions.cpp</c> (5090–5270).</summary>
public static class VmActorTerrainSetSizeModel
{
    public const int TelefragDamage = 9999;
    public const int ScrollStrifeCurrentSpecialId = 0;

    public static bool CheckTerrainOnFloor(bool onFloor, bool portalBlocksMovement) =>
        onFloor && portalBlocksMovement;

    public static bool ShouldInstantDeathTerrain(int damageAmount) => damageAmount >= TelefragDamage;

    public static (double Speed, double AngleDeg) StrifeCurrentThrust(int tagMinus100)
    {
        var angleSpeed = tagMinus100;
        var speed = (angleSpeed % 10) / 16.0;
        var angle = (angleSpeed / 10) * (360.0 / 8.0);
        return (speed, angle);
    }

    public static (double Radius, double Height) PlanSetSizeDimensions(double reqRadius, double reqHeight, double currentRadius, double currentHeight) =>
        (ResolveNewRadius(reqRadius, currentRadius), ResolveNewHeight(reqHeight, currentHeight));

    public static (double Radius, double Height) RevertSetSizeDimensions(double oldRadius, double oldHeight) =>
        (oldRadius, oldHeight);

    public static double ResolveNewRadius(double requested, double current) =>
        requested < 0 ? current : requested;

    public static double ResolveNewHeight(double requested, double current) =>
        requested < 0 ? current : requested;

    public static bool ShouldRevertSize(bool testPos, bool locationValid) => testPos && !locationValid;

    public static (object? Camera, bool Revert) PlanSetCameraTarget(object? requestedCamera, object self)
    {
        if (requestedCamera is null)
            return (self, false);
        return (requestedCamera, true);
    }

    public static bool SetCameraRequiresPlayer(bool hasPlayer, bool moIsSelf) => hasPlayer && moIsSelf;

    public static bool ShouldClearPastViewer(object? oldCamera, object? newCamera) =>
        !ReferenceEquals(oldCamera, newCamera);

    public static uint EnsureModelDataFlags(bool hadModel) => hadModel ? 1U : 0U;

    public static bool ShouldCleanupModelData(
        bool decoupledAnimations,
        int modelCount,
        int frameGeneratorCount,
        int skinIdCount,
        int animationIdCount,
        int boneOverrideCount,
        bool modelDefNull,
        uint flagsWithoutHadModel) =>
        !decoupledAnimations
        && modelCount == 0
        && frameGeneratorCount == 0
        && skinIdCount == 0
        && animationIdCount == 0
        && boneOverrideCount == 0
        && modelDefNull
        && flagsWithoutHadModel == 0;

    public static int? ResolveRuntimeModelId(int runtimeSize, int index, int modelId)
    {
        if (ModelIndexInRuntimeRange(runtimeSize, index, modelId))
            return modelId;
        return null;
    }

    public static bool TryResolveBoneModelIndex(
        bool decoupledAnimations,
        bool hasClassFrames,
        int runtimeSize,
        int index,
        int modelId,
        int baseFrameSize,
        out int resolvedModelId)
    {
        resolvedModelId = -1;
        if (!decoupledAnimations)
            return false;
        if (!hasClassFrames)
            return false;

        var runtime = ResolveRuntimeModelId(runtimeSize, index, modelId);
        if (runtime.HasValue)
        {
            resolvedModelId = runtime.Value;
            return true;
        }

        if (ModelIndexInBaseFrames(baseFrameSize, index))
        {
            resolvedModelId = index;
            return true;
        }

        return false;
    }

    public static bool ModelIndexInRuntimeRange(int runtimeSize, int index, int modelId) =>
        runtimeSize > index && modelId >= 0 && modelId < runtimeSize;

    public static bool ModelIndexInBaseFrames(int baseSize, int index) => baseSize > index;

    public static bool SetMugshotState_RequiresLocalView(bool localView) => localView;

    public readonly record struct SprayDecalParameters(
        string Name,
        double Dist,
        double OffsetX,
        double OffsetY,
        double OffsetZ,
        double DirectionX,
        double DirectionY,
        double DirectionZ,
        bool UseBloodColor,
        uint DecalColor,
        int Translation);

    public static SprayDecalParameters BuildSprayDecalParameters(
        string name,
        double dist,
        double offsetX,
        double offsetY,
        double offsetZ,
        double directionX,
        double directionY,
        double directionZ,
        bool useBloodColor,
        uint decalColor,
        int translation) =>
        new(name, dist, offsetX, offsetY, offsetZ, directionX, directionY, directionZ, useBloodColor, decalColor, translation);
}

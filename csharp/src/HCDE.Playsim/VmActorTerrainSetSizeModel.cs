namespace HCDE.Playsim;

/// <summary><c>A_CheckTerrain</c>, <c>A_SetSize</c>, <c>SetCamera</c>, model-data helpers from <c>p_actionfunctions.cpp</c> (5090–5270).</summary>
public static class VmActorTerrainSetSizeModel
{
    public const int TelefragDamage = 9999;

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

    public static double ResolveNewRadius(double requested, double current) =>
        requested < 0 ? current : requested;

    public static double ResolveNewHeight(double requested, double current) =>
        requested < 0 ? current : requested;

    public static bool ShouldRevertSize(bool testPos, bool locationValid) => testPos && !locationValid;

    public static bool ShouldUpdatePlayerFullHeight(bool hasPlayer, bool moIsSelf) =>
        hasPlayer && moIsSelf;

    public static bool SetCameraRequiresPlayer(bool hasPlayer, bool moIsSelf) =>
        hasPlayer && moIsSelf;

    public static bool UseSelfWhenCameraNull(bool cameraNull) => cameraNull;

    public static bool SetRevertPleaseWhen(bool revertFlag) => revertFlag;

    public static bool ShouldClearPastViewer(bool cameraChanged) => cameraChanged;

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

    public static bool RestoredHadModelFlag(uint flags) => (flags & 1U) != 0;

    public static bool SetGetBoneRequiresDecoupled(bool decoupledAnimations) => decoupledAnimations;

    public static bool HasModelDefEntry(bool hasModelDataDef, bool hasClassFrames) =>
        hasModelDataDef || hasClassFrames;

    public static bool ModelIndexInRuntimeRange(int runtimeSize, int index, int modelId) =>
        runtimeSize > index && modelId >= 0 && modelId < runtimeSize;

    public static bool ModelIndexInBaseFrames(int baseSize, int index) => baseSize > index;

    public static bool SprayDecal_ForwardsParameters(bool hasSelf) => hasSelf;

    public static bool SetMugshotState_RequiresLocalView(bool localView) => localView;
}

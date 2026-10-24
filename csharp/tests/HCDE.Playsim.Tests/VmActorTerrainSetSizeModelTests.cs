namespace HCDE.Playsim.Tests;

public class VmActorTerrainSetSizeModelTests
{
    [Fact]
    public void ShouldInstantDeathTerrain_AtTelefragThreshold() =>
        Assert.True(VmActorTerrainSetSizeModel.ShouldInstantDeathTerrain(9999));

    [Fact]
    public void ResolveNewRadius_NegativeKeepsCurrent() =>
        Assert.Equal(16, VmActorTerrainSetSizeModel.ResolveNewRadius(-1, 16));

    [Fact]
    public void ShouldRevertSize_WhenTestFails() =>
        Assert.True(VmActorTerrainSetSizeModel.ShouldRevertSize(true, false));

    [Fact]
    public void SetCameraRequiresPlayer_WhenLocalMo() =>
        Assert.True(VmActorTerrainSetSizeModel.SetCameraRequiresPlayer(true, true));

    [Fact]
    public void EnsureModelDataFlags_WhenHadModel() =>
        Assert.Equal(1U, VmActorTerrainSetSizeModel.EnsureModelDataFlags(true));

    [Fact]
    public void ShouldCleanupModelData_WhenEmpty() =>
        Assert.True(VmActorTerrainSetSizeModel.ShouldCleanupModelData(false, 0, 0, 0, 0, 0, true, 0));

    [Fact]
    public void ModelIndexInRuntimeRange_MatchesNativeGate() =>
        Assert.True(VmActorTerrainSetSizeModel.ModelIndexInRuntimeRange(3, 1, 2));

    [Fact]
    public void TryResolveBoneModelIndex_PrefersRuntimeModel() =>
        Assert.True(VmActorTerrainSetSizeModel.TryResolveBoneModelIndex(true, true, 5, 1, 2, 3, out var id) && id == 2);
}

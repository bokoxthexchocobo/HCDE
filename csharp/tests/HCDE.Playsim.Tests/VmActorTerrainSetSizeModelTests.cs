namespace HCDE.Playsim.Tests;

public class VmActorTerrainSetSizeModelTests
{
    [Fact]
    public void ShouldInstantDeathTerrain_AtTelefragThreshold() =>
        Assert.True(VmActorTerrainSetSizeModel.ShouldInstantDeathTerrain(9999));

    [Fact]
    public void PlanSetSizeDimensions_NegativeRequestsKeepCurrent() =>
        Assert.Equal((16, 32), VmActorTerrainSetSizeModel.PlanSetSizeDimensions(-1, -1, 16, 32));

    [Fact]
    public void ShouldRevertSize_WhenTestFails() =>
        Assert.True(VmActorTerrainSetSizeModel.ShouldRevertSize(true, false));

    [Fact]
    public void PlanSetCameraTarget_UsesSelfWhenNull()
    {
        var self = new object();
        var (cam, _) = VmActorTerrainSetSizeModel.PlanSetCameraTarget(null, self);
        Assert.Same(self, cam);
    }

    [Fact]
    public void SetCameraRequiresPlayer_WhenLocalMo() =>
        Assert.True(VmActorTerrainSetSizeModel.SetCameraRequiresPlayer(true, true));

    [Fact]
    public void SprayDecal_ForwardsParameters()
    {
        var p = VmActorTerrainSetSizeModel.BuildSprayDecalParameters(
            "test", 64, 1, 2, 3, 0, 0, 1, true, 0xFF00FF, 7);
        Assert.Equal("test", p.Name);
        Assert.Equal(64, p.Dist);
        Assert.True(p.UseBloodColor);
        Assert.Equal(7, p.Translation);
    }

    [Fact]
    public void SetMugshotState_RequiresLocalView() =>
        Assert.False(VmActorTerrainSetSizeModel.SetMugshotState_RequiresLocalView(false));

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
